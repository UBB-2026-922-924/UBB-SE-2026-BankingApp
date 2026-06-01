namespace BankingApp.Application.Features.Loans.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Features.Loans.Dtos;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using Shared.Persistence;

public sealed class LoansService(
    ILoanRepository loanRepository,
    IUnitOfWork unitOfWork)
    : ILoansService
{
    private const int MaxActiveLoans = 5;
    private const decimal TotalDebtLimit = 200000m;
    private const decimal PersonalLoanRate = 8.5m;
    private const decimal MortgageLoanRate = 4.5m;
    private const decimal StudentLoanRate = 3.0m;
    private const decimal AutoLoanRate = 6.5m;

    private readonly LoanApplicationValidator _validator = new();

    public async Task<ErrorOr<IReadOnlyCollection<Loan>>> GetLoansByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Loan> loans = await loanRepository.GetLoansByUserAsync(userId, cancellationToken);
        return ErrorOrFactory.From(loans);
    }

    public async Task<ErrorOr<Loan>> GetLoanByIdAsync(int loanId, CancellationToken cancellationToken = default)
    {
        Loan? loan = await loanRepository.GetLoanByIdAsync(loanId, cancellationToken);
        if (loan is null)
        {
            return LoanErrors.LoanNotFound;
        }

        return loan;
    }

    public async Task<ErrorOr<LoanApplicationResult>> SubmitApplicationAsync(LoanApplicationRequest request, CancellationToken cancellationToken = default)
    {
        ErrorOr<Success> validation = _validator.Validate(request);
        if (validation.IsError)
        {
            return new LoanApplicationResult
            {
                Status = LoanApplicationStatus.Rejected.ToString(),
                RejectionReason = validation.FirstError.Description,
            };
        }

        var application = LoanApplication.Create(
            request.UserId, request.LoanType, request.DesiredAmount, request.PreferredTermMonths, request.Purpose);

        int applicationId = await loanRepository.CreateLoanApplicationAsync(application, cancellationToken);

        IReadOnlyCollection<Loan> existing = await loanRepository.GetLoansByUserAsync(request.UserId, cancellationToken);
        decimal totalOutstanding = existing.Sum(l => l.OutstandingBalance);
        int activeCount = existing.Count(l => l.LoanStatus == LoanStatus.Active);

        if (activeCount >= MaxActiveLoans)
        {
            application.Reject("Maximum number of active loans reached.");
            await loanRepository.UpdateLoanApplicationStatusAsync(applicationId, LoanApplicationStatus.Rejected, application.RejectionReason, cancellationToken);
            return new LoanApplicationResult { Status = LoanApplicationStatus.Rejected.ToString(), RejectionReason = application.RejectionReason };
        }

        if (totalOutstanding + request.DesiredAmount >= TotalDebtLimit)
        {
            application.Reject("Total debt limit exceeded.");
            await loanRepository.UpdateLoanApplicationStatusAsync(applicationId, LoanApplicationStatus.Rejected, application.RejectionReason, cancellationToken);
            return new LoanApplicationResult { Status = LoanApplicationStatus.Rejected.ToString(), RejectionReason = application.RejectionReason };
        }

        application.Approve();
        await loanRepository.UpdateLoanApplicationStatusAsync(applicationId, LoanApplicationStatus.Approved, null, cancellationToken);

        decimal rate = GetInterestRate(request.LoanType);
        LoanEstimate estimate = AmortizationCalculator.ComputeEstimate(request.DesiredAmount, rate, request.PreferredTermMonths);

        var loan = Loan.Create(request.UserId, request.LoanType, request.DesiredAmount, rate, estimate.MonthlyInstallment, request.PreferredTermMonths, DateTime.UtcNow);
        int loanId = await loanRepository.CreateLoanAsync(loan, cancellationToken);

        IReadOnlyCollection<AmortizationRow> schedule = AmortizationCalculator.Generate(loan);
        await loanRepository.SaveAmortizationAsync(schedule, cancellationToken);

        return new LoanApplicationResult { Status = LoanApplicationStatus.Approved.ToString() };
    }

    public ErrorOr<LoanEstimate> GetEstimate(LoanApplicationRequest request)
    {
        ErrorOr<Success> validation = _validator.Validate(request);
        if (validation.IsError)
        {
            return validation.FirstError;
        }

        decimal rate = GetInterestRate(request.LoanType);
        return AmortizationCalculator.ComputeEstimate(request.DesiredAmount, rate, request.PreferredTermMonths);
    }

    public async Task<ErrorOr<Success>> PayInstallmentAsync(int loanId, decimal? customAmount, CancellationToken cancellationToken = default)
    {
        Loan? loan = await loanRepository.GetLoanByIdAsync(loanId, cancellationToken);
        if (loan is null)
        {
            return LoanErrors.LoanNotFound;
        }

        decimal minimumDue = Math.Min(loan.MonthlyInstallment, loan.OutstandingBalance);
        decimal paymentAmount = customAmount ?? minimumDue;

        ErrorOr<Success> payResult = loan.PayInstallment(paymentAmount);
        if (payResult.IsError)
        {
            return payResult.FirstError;
        }

        await loanRepository.UpdateLoanAfterPaymentAsync(loanId, loan.OutstandingBalance, loan.RemainingMonths, loan.LoanStatus, cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<IReadOnlyCollection<AmortizationRow>>> GetAmortizationAsync(int loanId, CancellationToken cancellationToken = default)
    {
        Loan? loan = await loanRepository.GetLoanByIdAsync(loanId, cancellationToken);
        if (loan is null)
        {
            return LoanErrors.LoanNotFound;
        }

        IReadOnlyCollection<AmortizationRow> rows = await loanRepository.GetAmortizationAsync(loanId, cancellationToken);

        if (rows.Count == 0)
        {
            IReadOnlyCollection<AmortizationRow> generated = AmortizationCalculator.Generate(loan);
            await loanRepository.SaveAmortizationAsync(generated, cancellationToken);
            rows = generated;
        }

        return ErrorOrFactory.From(rows);
    }

    private static decimal GetInterestRate(LoanType loanType) => loanType switch
    {
        LoanType.Personal => PersonalLoanRate,
        LoanType.Mortgage => MortgageLoanRate,
        LoanType.Student => StudentLoanRate,
        LoanType.Auto => AutoLoanRate,
        _ => PersonalLoanRate,
    };
}
