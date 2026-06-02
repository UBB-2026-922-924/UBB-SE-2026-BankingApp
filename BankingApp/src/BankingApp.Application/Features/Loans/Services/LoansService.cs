namespace BankingApp.Application.Features.Loans.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Contracts.Features.Loans.Dtos;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;
using Domain.Enums;
using ErrorOr;

public class LoansService : ILoansService
{
    private const int MinimumIdExclusive = 0;
    private const decimal ZeroAmount = 0m;
    private const int NoRowsCount = 0;
    private const int MaxActiveLoans = 5;
    private const decimal TotalDebtLimit = 200000m;
    private const decimal PersonalLoanRate = 8.5m;
    private const decimal MortgageLoanRate = 4.5m;
    private const decimal StudentLoanRate = 3.0m;
    private const decimal AutoLoanRate = 6.5m;

    private readonly ILoansRepoProxy _loanRepoProxy;
    private readonly ILoanDialogStateRepoProxy _loanDialogState;
    private readonly ILoanApplicationPresentationRepoProxy _loanApplicationPresentation;
    private readonly LoanApplicationValidator _validator;
    private readonly PaymentCalculationService _paymentCalculationService;

    public LoansService(
        ILoansRepoProxy loanRepoProxy,
        ILoanDialogStateRepoProxy loanDialogState,
        ILoanApplicationPresentationRepoProxy loanApplicationPresentation)
    {
        _loanRepoProxy = loanRepoProxy ?? throw new ArgumentNullException(nameof(loanRepoProxy));
        _loanDialogState = loanDialogState ?? throw new ArgumentNullException(nameof(loanDialogState));
        _loanApplicationPresentation = loanApplicationPresentation ?? throw new ArgumentNullException(nameof(loanApplicationPresentation));
        _validator = new LoanApplicationValidator();
        _paymentCalculationService = new PaymentCalculationService();
    }

    public Task<List<Loan>> GetLoansByUserAsync(int userId)
    {
        if (userId <= MinimumIdExclusive)
        {
            return Task.FromResult<List<Loan>>([]);
        }

        return _loanRepoProxy.GetLoansByUserAsync(userId);
    }

    public async Task<LoanApplicationResult> SubmitLoanApplicationAsync(LoanApplicationRequest request)
    {
        ErrorOr<Success> validation = _validator.Validate(request);
        if (validation.IsError)
        {
            return new LoanApplicationResult
            {
                Status = LoanApplicationStatus.Rejected,
                RejectionReason = validation.FirstError.Description,
            };
        }

        int applicationId = await _loanRepoProxy.CreateLoanApplicationAsync(request);
        if (applicationId <= MinimumIdExclusive)
        {
            return new LoanApplicationResult
            {
                Status = LoanApplicationStatus.Rejected,
                RejectionReason = "Could not create loan application.",
            };
        }

        var application = LoanApplication.Create(
            request.UserId,
            request.LoanType,
            request.DesiredAmount,
            request.PreferredTermMonths,
            request.Purpose);

        (LoanApplicationStatus status, string? reason) = await EvaluateApplicationAsync(application);
        await _loanRepoProxy.UpdateLoanApplicationStatusAsync(applicationId, status, reason);

        if (status == LoanApplicationStatus.Approved)
        {
            int loanId = await CreateApprovedLoanAsync(application);
            await EnsureAmortizationAsync(loanId);
        }

        return new LoanApplicationResult
        {
            Status = status,
            RejectionReason = reason,
        };
    }

    public LoanEstimate GetLoanEstimate(LoanApplicationRequest request)
    {
        ErrorOr<Success> validation = _validator.Validate(request);
        if (validation.IsError)
        {
            return new LoanEstimate();
        }

        decimal rate = GetInterestRateForType(request.LoanType);
        return AmortizationCalculator.ComputeEstimate(
            request.DesiredAmount,
            rate,
            request.PreferredTermMonths);
    }

    public async Task PayInstallmentAsync(int loanId, decimal? customAmount)
    {
        var loan = await _loanRepoProxy.GetLoanByIdAsync(loanId);
        if (loan == null)
        {
            throw new InvalidOperationException("Loan not found.");
        }

        if (loan.RemainingMonths <= MinimumIdExclusive || loan.LoanStatus == LoanStatus.Passed)
        {
            throw new InvalidOperationException("This loan is already closed.");
        }

        decimal minimumDue = GetMinimumDue(loan.MonthlyInstallment, loan.OutstandingBalance);
        decimal paymentAmount = customAmount ?? minimumDue;

        if (paymentAmount <= ZeroAmount)
        {
            throw new ArgumentException("Payment amount must be greater than zero.");
        }

        if (customAmount.HasValue && paymentAmount < minimumDue)
        {
            throw new InvalidOperationException("Payment amount must be at least the amount currently due.");
        }

        if (paymentAmount > loan.OutstandingBalance)
        {
            throw new InvalidOperationException("Payment amount exceeds the outstanding balance.");
        }

        (decimal newBalance, int newRemainingMonths) = CalculatePaymentPreview(
            loan.MonthlyInstallment,
            loan.OutstandingBalance,
            loan.RemainingMonths,
            isStandardPayment: !customAmount.HasValue,
            customPaymentAmount: paymentAmount);

        LoanStatus newStatus = newBalance <= ZeroAmount || newRemainingMonths == MinimumIdExclusive
            ? LoanStatus.Passed
            : loan.LoanStatus;

        await _loanRepoProxy.UpdateLoanAfterPaymentAsync(loanId, newBalance, newRemainingMonths, newStatus);
    }

    public decimal? ParseCustomPaymentAmount(string input)
    {
        (bool success, decimal amount) = _paymentCalculationService.ParsePaymentAmount(input);
        return success ? amount : null;
    }

    public decimal NormalizeCustomPaymentAmount(Loan loan, decimal? currentCustomAmount)
    {
        return _paymentCalculationService.GetInitialCustomAmount(
            loan.MonthlyInstallment,
            loan.OutstandingBalance,
            currentCustomAmount.HasValue ? (double?)currentCustomAmount.Value : null);
    }

    public double GetRepaymentProgress(Loan loan)
    {
        return (double)AmortizationCalculator.ComputeRepaymentProgress(loan.Principal, loan.OutstandingBalance);
    }

    public async Task<List<AmortizationRow>> GetAmortizationAsync(int loanId)
    {
        var rows = await _loanRepoProxy.GetAmortizationAsync(loanId);
        if (rows == null || rows.Count == NoRowsCount)
        {
            await EnsureAmortizationAsync(loanId);
            rows = await _loanRepoProxy.GetAmortizationAsync(loanId);
        }

        MarkCurrentRow(rows);
        return rows;
    }

    public Task<BuildApplicationOutcomeResponse?> GetBuildApplicationOutcomeAsync(string? rejectionReason) =>
        _loanApplicationPresentation.GetBuildApplicationOutcome(rejectionReason);

    public Task<bool> GetShouldComputeEstimateAsync(double desiredAmount, int preferredTermMonths, string purpose) =>
        _loanDialogState.GetShouldComputeEstimate(desiredAmount, preferredTermMonths, purpose);

    private async Task EnsureAmortizationAsync(int loanId)
    {
        var loan = await _loanRepoProxy.GetLoanByIdAsync(loanId);
        List<AmortizationRow> rows = AmortizationCalculator.Generate(loan);
        await _loanRepoProxy.SaveAmortizationAsync(loanId, rows);
    }

    private static void MarkCurrentRow(List<AmortizationRow> rows)
    {
        bool isCurrentSet = false;
        foreach (AmortizationRow row in rows)
        {
            if (!isCurrentSet && row.DueDate.Date >= DateTime.Today)
            {
                row.MarkAsCurrent();
                isCurrentSet = true;
            }
            else
            {
                row.ClearCurrent();
            }
        }
    }

    private async Task<(LoanApplicationStatus approved, string? reason)> EvaluateApplicationAsync(LoanApplication application)
    {
        var currentLoans = await _loanRepoProxy.GetLoansByUserAsync(application.UserId);
        var totalOutstanding = currentLoans.Sum(loan => loan.OutstandingBalance);
        int activeLoansCount = currentLoans.Count(loan => loan.LoanStatus == LoanStatus.Active);

        if (activeLoansCount >= MaxActiveLoans)
        {
            return (LoanApplicationStatus.Rejected, "Maximum number of active loans reached.");
        }

        if (totalOutstanding + application.DesiredAmount >= TotalDebtLimit)
        {
            return (LoanApplicationStatus.Rejected, "Total debt limit exceeded.");
        }

        return (LoanApplicationStatus.Approved, null);
    }

    private async Task<int> CreateApprovedLoanAsync(LoanApplication application)
    {
        decimal rate = GetInterestRateForType(application.LoanType);
        LoanEstimate estimate = AmortizationCalculator.ComputeEstimate(
            application.DesiredAmount,
            rate,
            application.PreferredTermMonths);

        var loan = Loan.Create(
            application.UserId,
            application.LoanType,
            application.DesiredAmount,
            rate,
            estimate.MonthlyInstallment,
            application.PreferredTermMonths,
            DateTime.UtcNow);

        return await _loanRepoProxy.CreateLoanAsync(loan);
    }

    private static decimal GetInterestRateForType(LoanType loanType)
    {
        return loanType switch
        {
            LoanType.Personal => PersonalLoanRate,
            LoanType.Mortgage => MortgageLoanRate,
            LoanType.Student => StudentLoanRate,
            LoanType.Auto => AutoLoanRate,
            _ => PersonalLoanRate,
        };
    }

    private static decimal GetMinimumDue(decimal monthlyInstallment, decimal outstandingBalance)
    {
        return Math.Min(monthlyInstallment, outstandingBalance);
    }

    private (decimal BalanceAfterPayment, int RemainingMonths) CalculatePaymentPreview(
        decimal monthlyInstallment,
        decimal outstandingBalance,
        int remainingMonths,
        bool isStandardPayment,
        decimal customPaymentAmount)
    {
        return _paymentCalculationService.CalculatePaymentPreview(
            monthlyInstallment,
            outstandingBalance,
            remainingMonths,
            isStandardPayment,
            customPaymentAmount);
    }

    private sealed class PaymentCalculationService
    {
        private const decimal LocalZeroAmount = 0m;
        private const int ZeroMonths = 0;
        private const int SingleMonth = 1;

        public (decimal BalanceAfterPayment, int RemainingMonths) CalculatePaymentPreview(
            decimal monthlyInstallment,
            decimal outstandingBalance,
            int remainingMonths,
            bool isStandardPayment,
            decimal customPaymentAmount = LocalZeroAmount)
        {
            decimal minimumDue = GetMinimumDue(monthlyInstallment, outstandingBalance);
            decimal paymentAmount = isStandardPayment ? minimumDue : customPaymentAmount;
            decimal balanceAfterPayment = Math.Max(LocalZeroAmount, outstandingBalance - paymentAmount);

            if (balanceAfterPayment <= LocalZeroAmount)
            {
                return (LocalZeroAmount, ZeroMonths);
            }

            int monthsPaid = isStandardPayment
                ? SingleMonth
                : paymentAmount <= LocalZeroAmount
                    ? ZeroMonths
                    : (int)Math.Floor(paymentAmount / monthlyInstallment);

            int newRemainingMonths = Math.Max(ZeroMonths, remainingMonths - monthsPaid);
            return (balanceAfterPayment, newRemainingMonths);
        }

        public (bool Success, decimal Amount) ParsePaymentAmount(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return (false, LocalZeroAmount);
            }

            if (decimal.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out decimal currentCultureResult))
            {
                return (true, currentCultureResult);
            }

            if (decimal.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal invariantCultureResult))
            {
                return (true, invariantCultureResult);
            }

            return (false, LocalZeroAmount);
        }

        public decimal GetInitialCustomAmount(
            decimal monthlyInstallment,
            decimal outstandingBalance,
            double? currentCustomAmount)
        {
            decimal amount = currentCustomAmount.HasValue ? (decimal)currentCustomAmount.Value : monthlyInstallment;
            return amount > outstandingBalance ? outstandingBalance : amount;
        }

        private static decimal GetMinimumDue(decimal monthlyInstallment, decimal outstandingBalance)
        {
            return Math.Min(monthlyInstallment, outstandingBalance);
        }
    }
}
