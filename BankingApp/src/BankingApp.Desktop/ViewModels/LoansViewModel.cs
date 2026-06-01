// <copyright file="LoansViewModel.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Features.Loans.Dtos;
using Domain.Enums;
using Domain.Aggregates.LoanAggregate;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Aggregates.LoanAggregate.Entities;
using Infrastructure.Http.Features.Loans.Services;
using ErrorOr;
using Shared.Enums;

public partial class LoansViewModel : ObservableObject
{
    private const string CustomAmountDisplayFormat = "0.##";
    private const int ZeroCount = 0;
    private const decimal ZeroAmount = 0m;
    private const int FirstPage = 1;
    private const int DefaultPageSize = 10;

    private readonly ILoanApplicationPresentationRepoProxy _loanApplicationPresentationRepoProxy;
    private readonly ILoanDialogStateRepoProxy _loanDialogStateRepoProxy;
    private readonly ILoansRepoProxy _loansRepoProxy;
    private readonly PdfExporter _pdfExporter;

    [ObservableProperty]
    private ObservableCollection<AmortizationRow> _amortizationRows = new ObservableCollection<AmortizationRow>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ApplicationWasApproved))]
    private string _applicationResult = string.Empty;

    [ObservableProperty]
    private bool _applicationWasApproved;

    [ObservableProperty]
    private LoanEstimate _currentEstimate;

    [ObservableProperty]
    private double? _customAmount;

    [ObservableProperty]
    private double _desiredAmount;

    [ObservableProperty]
    private string _dialogActionText = "Continue";

    [ObservableProperty]
    private string _dialogTitle = "Apply for Loan";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEstimateVisible;

    [ObservableProperty]
    private bool _isFormVisible = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isReviewVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredLoans))]
    private ObservableCollection<LoanViewModel> _loans = new ObservableCollection<LoanViewModel>();

    [ObservableProperty]
    private decimal _paymentPreviewBalance;

    [ObservableProperty]
    private int _paymentPreviewRemainingMonths;

    [ObservableProperty]
    private int _preferredTermMonths;

    [ObservableProperty]
    private string _purpose = string.Empty;

    [ObservableProperty]
    private LoanViewModel _selectedLoan;

    [ObservableProperty]
    private LoanType _selectedLoanType;

    [NotifyPropertyChangedFor(nameof(FilteredLoans))]
    [ObservableProperty]
    private LoanStatus? _statusFilter;

    [NotifyPropertyChangedFor(nameof(FilteredLoans))]
    [ObservableProperty]
    private LoanType? _typeFilter;

    [ObservableProperty]
    private User _currentUser;

    [ObservableProperty]
    private LoansState _state = LoansState.Idle;

    public LoansViewModel(
        ILoansRepoProxy loansRepoProxy,
        ILoanDialogStateRepoProxy loanDialogStateRepoProxy,
        ILoanApplicationPresentationRepoProxy loanApplicationPresentationRepoProxy)
    {
        this._loansRepoProxy = loansRepoProxy ?? throw new ArgumentNullException(nameof(loansRepoProxy));
        this._loanDialogStateRepoProxy = loanDialogStateRepoProxy ?? throw new ArgumentNullException(nameof(loanDialogStateRepoProxy));
        this._loanApplicationPresentationRepoProxy = loanApplicationPresentationRepoProxy ?? throw new ArgumentNullException(nameof(loanApplicationPresentationRepoProxy));
        _pdfExporter = new PdfExporter();
    }

    public IEnumerable<LoanType> LoanTypes => Enum.GetValues<LoanType>();

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public IEnumerable<LoanViewModel> FilteredLoans =>
        _loans.Where(loan =>
            (_statusFilter == null || loan.Loan.LoanStatus == _statusFilter) &&
            (_typeFilter == null || loan.Loan.LoanType == _typeFilter));

    public LoanViewModel CreateLoanViewModel(Loan loan)
    {
        return new LoanViewModel(loan, GetRepaymentProgress(loan));
    }

    public void SelectLoan(Loan loan)
    {
        SelectedLoan = CreateLoanViewModel(loan);
    }

    [RelayCommand]
    public async Task LoadLoansAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        State = LoansState.Loading;
        try
        {
            List<Loan> result = await _loansRepoProxy.GetLoansByUserAsync(CurrentUser.Id);
            var loanViewModels = new List<LoanViewModel>();
            foreach (Loan loan in result)
            {
                loanViewModels.Add(CreateLoanViewModel(loan));
            }

            Loans = new ObservableCollection<LoanViewModel>(loanViewModels);
            State = LoansState.Ready;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            State = LoansState.Error;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ApplyForLoanAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var request = new LoanApplicationRequest
            {
                UserId = CurrentUser.Id,
                LoanType = SelectedLoanType,
                DesiredAmount = (decimal)DesiredAmount,
                PreferredTermMonths = PreferredTermMonths,
                Purpose = Purpose,
            };

            int applicationId = await _loansRepoProxy.CreateLoanApplicationAsync(request);

            BuildApplicationOutcomeResponse? applicationOutcome = await _loanApplicationPresentationRepoProxy.GetBuildApplicationOutcome(
                applicationId > 0 ? null : "Rejected");
            ApplicationResult = applicationOutcome?.Message ?? (applicationId > 0 ? "Loan application approved." : "Loan application rejected.");
            ApplicationWasApproved = applicationOutcome?.IsApproved ?? applicationId > 0;
            if (ApplicationWasApproved)
            {
                await LoadLoansAsync();
                OnPropertyChanged(nameof(FilteredLoans));
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            State = LoansState.Error;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ComputeLiveEstimate()
    {
        ErrorMessage = string.Empty;
        try
        {
            var request = new LoanApplicationRequest
            {
                UserId = CurrentUser.Id,
                LoanType = SelectedLoanType,
                DesiredAmount = (decimal)DesiredAmount,
                PreferredTermMonths = PreferredTermMonths,
                Purpose = Purpose,
            };
            CurrentEstimate = ComputeEstimate(request.DesiredAmount, GetIndicativeRate(request.LoanType), request.PreferredTermMonths);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public async Task PayInstallmentAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            decimal? amount = CustomAmount.HasValue
                ? (decimal?)CustomAmount.Value
                : null;
            Loan loan = SelectedLoan.Loan;
            decimal paymentAmount = amount ?? Math.Min(loan.MonthlyInstallment, loan.OutstandingBalance);
            ErrorOr<Success> paymentResult = loan.PayInstallment(paymentAmount);
            if (paymentResult.IsError)
            {
                ErrorMessage = paymentResult.FirstError.Description;
                throw new InvalidOperationException(ErrorMessage);
            }

            await _loansRepoProxy.UpdateLoanAfterPaymentAsync(
                loan.Id,
                loan.OutstandingBalance,
                loan.RemainingMonths,
                loan.LoanStatus);
            await LoadLoansAsync();

            OnPropertyChanged(nameof(FilteredLoans));
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void UpdatePaymentPreview(bool isStandardPayment, string customAmountText = "")
    {
        if (SelectedLoan == null)
        {
            PaymentPreviewBalance = ZeroAmount;
            PaymentPreviewRemainingMonths = ZeroCount;
            return;
        }

        decimal? customAmount = null;
        if (!isStandardPayment)
        {
            customAmount = ParseCustomPaymentAmount(customAmountText);
        }

        (decimal balance, int months) = CalculatePaymentPreview(SelectedLoan.Loan, customAmount);
        PaymentPreviewBalance = balance;
        PaymentPreviewRemainingMonths = months;
    }

    public void SelectStandardPayment()
    {
        CustomAmount = null;
        UpdatePaymentPreview(true);
    }

    public string SelectCustomPayment()
    {
        if (SelectedLoan == null)
        {
            CustomAmount = null;
            UpdatePaymentPreview(false, string.Empty);
            return string.Empty;
        }

        decimal normalizedCustomAmount = NormalizeCustomPaymentAmount(
            SelectedLoan.Loan,
            CustomAmount.HasValue ? (decimal?)CustomAmount.Value : null);

        CustomAmount = (double)normalizedCustomAmount;

        string currentText = normalizedCustomAmount.ToString(CustomAmountDisplayFormat, CultureInfo.CurrentCulture);
        UpdatePaymentPreview(false, currentText);
        return currentText;
    }

    public void UpdateCustomPayment(string customAmountText)
    {
        decimal? parsedAmount = ParseCustomPaymentAmount(customAmountText);
        CustomAmount = parsedAmount.HasValue ? (double)parsedAmount.Value : null;
        UpdatePaymentPreview(false, customAmountText);
    }

    public async Task LoadAmortizationAsync()
    {
        if (SelectedLoan == null)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            List<AmortizationRow> rows = await _loansRepoProxy.GetAmortizationAsync(SelectedLoan.Loan.Id);
            AmortizationRows = new ObservableCollection<AmortizationRow>(rows);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DownloadSchedulePdfAsync()
    {
        try
        {
            List<AmortizationRow> rows = await _loansRepoProxy.GetAmortizationAsync(SelectedLoan.Loan.Id);
            var pdfBytes = _pdfExporter.ExportAmortization(rows);
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string fileName = $"amortization_schedule_{SelectedLoan.Loan.Id}.pdf";
            string filePath = Path.Combine(desktopPath, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true,
                });
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public void SwitchToReviewStage()
    {
        IsFormVisible = false;
        IsReviewVisible = true;
        DialogTitle = "Application Review";
        DialogActionText = "Submit";
    }

    public void ResetDialogState()
    {
        IsFormVisible = true;
        IsReviewVisible = false;
        DialogTitle = "Apply for Loan";
        DialogActionText = "Continue";
        ApplicationResult = string.Empty;
        ApplicationWasApproved = false;

        DesiredAmount = default;
        PreferredTermMonths = default;
        Purpose = string.Empty;
        CurrentEstimate = null;
        IsEstimateVisible = false;
    }

    partial void OnDesiredAmountChanged(double value)
    {
        TryComputeEstimate();
    }

    partial void OnPreferredTermMonthsChanged(int value)
    {
        TryComputeEstimate();
    }

    partial void OnSelectedLoanTypeChanged(LoanType value)
    {
        TryComputeEstimate();
    }

    partial void OnPurposeChanged(string value)
    {
        TryComputeEstimate();
    }

    private void TryComputeEstimate()
    {
        _ = TryComputeEstimateAsync();
    }

    private async Task TryComputeEstimateAsync()
    {
        bool isFullyFilled = await _loanDialogStateRepoProxy.GetShouldComputeEstimate(
            DesiredAmount,
            PreferredTermMonths,
            Purpose);

        if (isFullyFilled)
        {
            await ComputeLiveEstimate();
            IsEstimateVisible = true;
        }
        else
        {
            CurrentEstimate = null;
            IsEstimateVisible = false;
        }
    }

    private static (decimal BalanceAfterPayment, int RemainingMonths) CalculatePaymentPreview(
        Loan loan,
        decimal? customAmount = null)
    {
        decimal minimumDue = Math.Min(loan.MonthlyInstallment, loan.OutstandingBalance);
        decimal paymentAmount = customAmount ?? minimumDue;
        decimal balanceAfterPayment = Math.Max(ZeroAmount, loan.OutstandingBalance - paymentAmount);

        if (balanceAfterPayment <= ZeroAmount)
        {
            return (ZeroAmount, ZeroCount);
        }

        int monthsPaid = customAmount.HasValue
            ? paymentAmount <= ZeroAmount
                ? ZeroCount
                : (int)Math.Floor(paymentAmount / loan.MonthlyInstallment)
            : 1;
        int newRemainingMonths = Math.Max(ZeroCount, loan.RemainingMonths - monthsPaid);
        return (balanceAfterPayment, newRemainingMonths);
    }

    private static LoanEstimate ComputeEstimate(decimal amount, decimal annualRate, int termMonths)
    {
        if (amount <= ZeroAmount || termMonths <= ZeroCount)
        {
            return new LoanEstimate(annualRate, ZeroAmount, ZeroAmount);
        }

        decimal monthlyRate = annualRate / 12m / 100m;
        decimal monthlyInstallment = monthlyRate == ZeroAmount
            ? amount / termMonths
            : amount * monthlyRate * (decimal)Math.Pow((double)(1m + monthlyRate), termMonths) /
              ((decimal)Math.Pow((double)(1m + monthlyRate), termMonths) - 1m);

        monthlyInstallment = Math.Round(monthlyInstallment, 2);
        return new LoanEstimate(annualRate, monthlyInstallment, Math.Round(monthlyInstallment * termMonths, 2));
    }

    private static decimal GetIndicativeRate(LoanType loanType)
    {
        return loanType switch
        {
            LoanType.Mortgage => 5.25m,
            LoanType.Student => 4.50m,
            LoanType.Auto => 6.75m,
            _ => 8.50m
        };
    }

    private static double GetRepaymentProgress(Loan loan)
    {
        if (loan.Principal <= ZeroAmount)
        {
            return 0;
        }

        decimal paidAmount = Math.Max(ZeroAmount, loan.Principal - loan.OutstandingBalance);
        decimal progress = paidAmount / loan.Principal;
        return (double)Math.Clamp(progress, ZeroAmount, 1m);
    }

    private static decimal? ParseCustomPaymentAmount(string customAmountText)
    {
        return decimal.TryParse(customAmountText, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal value)
            && value > ZeroAmount
                ? value
                : null;
    }

    private static decimal NormalizeCustomPaymentAmount(Loan loan, decimal? amount)
    {
        decimal minimumDue = Math.Min(loan.MonthlyInstallment, loan.OutstandingBalance);
        decimal requestedAmount = amount ?? minimumDue;
        return Math.Min(Math.Max(requestedAmount, minimumDue), loan.OutstandingBalance);
    }
}
