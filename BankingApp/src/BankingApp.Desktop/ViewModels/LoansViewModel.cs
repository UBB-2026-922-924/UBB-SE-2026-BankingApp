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

/// <summary>
///     Coordinates loan account display, applications, payments, and amortization schedules.
/// </summary>
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

    /// <summary>Gets or sets the amortization rows for the selected loan.</summary>
    [ObservableProperty]
    public partial ObservableCollection<AmortizationRow> AmortizationRows { get; set; } = new ObservableCollection<AmortizationRow>();

    /// <summary>Gets or sets the loan application result message.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ApplicationWasApproved))]
    public partial string ApplicationResult { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the application was approved.</summary>
    [ObservableProperty]
    public partial bool ApplicationWasApproved { get; set; }

    /// <summary>Gets or sets the current loan estimate.</summary>
    [ObservableProperty]
    public partial LoanEstimate? CurrentEstimate { get; set; }

    /// <summary>Gets or sets the custom payment amount.</summary>
    [ObservableProperty]
    public partial double? CustomAmount { get; set; }

    /// <summary>Gets or sets the desired loan amount.</summary>
    [ObservableProperty]
    public partial double DesiredAmount { get; set; }

    /// <summary>Gets or sets the dialog action text.</summary>
    [ObservableProperty]
    public partial string DialogActionText { get; set; } = "Continue";

    /// <summary>Gets or sets the dialog title.</summary>
    [ObservableProperty]
    public partial string DialogTitle { get; set; } = "Apply for Loan";

    /// <summary>Gets or sets the current error message.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the estimate is visible.</summary>
    [ObservableProperty]
    public partial bool IsEstimateVisible { get; set; }

    /// <summary>Gets or sets a value indicating whether the form is visible.</summary>
    [ObservableProperty]
    public partial bool IsFormVisible { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether loan data is loading.</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>Gets or sets a value indicating whether the review stage is visible.</summary>
    [ObservableProperty]
    public partial bool IsReviewVisible { get; set; }

    /// <summary>Gets or sets the loaded loans.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredLoans))]
    public partial ObservableCollection<LoanViewModel> Loans { get; set; } = new ObservableCollection<LoanViewModel>();

    /// <summary>Gets or sets the payment preview balance.</summary>
    [ObservableProperty]
    public partial decimal PaymentPreviewBalance { get; set; }

    /// <summary>Gets or sets the payment preview remaining months.</summary>
    [ObservableProperty]
    public partial int PaymentPreviewRemainingMonths { get; set; }

    /// <summary>Gets or sets the preferred term in months.</summary>
    [ObservableProperty]
    public partial int PreferredTermMonths { get; set; }

    /// <summary>Gets or sets the loan purpose.</summary>
    [ObservableProperty]
    public partial string Purpose { get; set; } = string.Empty;

    /// <summary>Gets or sets the selected loan.</summary>
    [ObservableProperty]
    public partial LoanViewModel? SelectedLoan { get; set; }

    /// <summary>Gets or sets the selected loan type.</summary>
    [ObservableProperty]
    public partial LoanType SelectedLoanType { get; set; }

    /// <summary>Gets or sets the selected loan status filter.</summary>
    [NotifyPropertyChangedFor(nameof(FilteredLoans))]
    [ObservableProperty]
    public partial LoanStatus? StatusFilter { get; set; }

    /// <summary>Gets or sets the selected loan type filter.</summary>
    [NotifyPropertyChangedFor(nameof(FilteredLoans))]
    [ObservableProperty]
    public partial LoanType? TypeFilter { get; set; }

    /// <summary>Gets or sets the current user id.</summary>
    [ObservableProperty]
    public partial int CurrentUserId { get; set; }

    /// <summary>Gets or sets the current loans state.</summary>
    [ObservableProperty]
    public partial LoansState State { get; set; } = LoansState.Idle;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoansViewModel" /> class.
    /// </summary>
    /// <param name="loansRepoProxy">The loans HTTP proxy.</param>
    /// <param name="loanDialogStateRepoProxy">The loan dialog state proxy.</param>
    /// <param name="loanApplicationPresentationRepoProxy">The loan presentation proxy.</param>
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

    /// <summary>Gets the available loan types.</summary>
    public IEnumerable<LoanType> LoanTypes => Enum.GetValues<LoanType>();

    /// <summary>Gets a value indicating whether an error message is present.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>Gets the loans after the selected filters are applied.</summary>
    public IEnumerable<LoanViewModel> FilteredLoans =>
        Loans.Where(loan =>
            (StatusFilter == null || loan.Loan.LoanStatus == StatusFilter) &&
            (TypeFilter == null || loan.Loan.LoanType == TypeFilter));

    /// <summary>Creates a display view model for a loan.</summary>
    /// <param name="loan">The source loan.</param>
    /// <returns>The display view model.</returns>
    public LoanViewModel CreateLoanViewModel(Loan loan)
    {
        return new LoanViewModel(loan, GetRepaymentProgress(loan));
    }

    /// <summary>Selects a loan for downstream workflows.</summary>
    /// <param name="loan">The selected loan.</param>
    public void SelectLoan(Loan loan)
    {
        SelectedLoan = CreateLoanViewModel(loan);
    }

    /// <summary>Loads loans for the current user.</summary>
    [RelayCommand]
    public async Task LoadLoansAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        State = LoansState.Loading;
        try
        {
            List<Loan> result = await _loansRepoProxy.GetLoansByUserAsync(CurrentUserId);
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

    /// <summary>Submits the current loan application.</summary>
    [RelayCommand]
    public async Task ApplyForLoanAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var request = new LoanApplicationRequest
            {
                UserId = CurrentUserId,
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

    /// <summary>Computes a live estimate for the current application form.</summary>
    [RelayCommand]
    public async Task ComputeLiveEstimate()
    {
        ErrorMessage = string.Empty;
        try
        {
            var request = new LoanApplicationRequest
            {
                UserId = CurrentUserId,
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

    /// <summary>Pays an installment for the selected loan.</summary>
    public async Task PayInstallmentAsync()
    {
        if (SelectedLoan is null)
        {
            ErrorMessage = "No loan selected.";
            return;
        }

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

    /// <summary>Updates the payment preview for the selected loan.</summary>
    /// <param name="isStandardPayment">Whether the standard monthly payment is selected.</param>
    /// <param name="customAmountText">The custom payment amount text.</param>
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

    /// <summary>Selects the standard payment amount.</summary>
    public void SelectStandardPayment()
    {
        CustomAmount = null;
        UpdatePaymentPreview(true);
    }

    /// <summary>Selects and normalizes the custom payment amount.</summary>
    /// <returns>The normalized custom payment text.</returns>
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

    /// <summary>Updates the selected custom payment amount.</summary>
    /// <param name="customAmountText">The custom payment amount text.</param>
    public void UpdateCustomPayment(string customAmountText)
    {
        decimal? parsedAmount = ParseCustomPaymentAmount(customAmountText);
        CustomAmount = parsedAmount.HasValue ? (double)parsedAmount.Value : null;
        UpdatePaymentPreview(false, customAmountText);
    }

    /// <summary>Loads the amortization schedule for the selected loan.</summary>
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

    /// <summary>Downloads the amortization schedule as a PDF.</summary>
    [RelayCommand]
    public async Task DownloadSchedulePdfAsync()
    {
        if (SelectedLoan is null)
        {
            ErrorMessage = "No loan selected.";
            return;
        }

        try
        {
            List<AmortizationRow> rows = await _loansRepoProxy.GetAmortizationAsync(SelectedLoan.Loan.Id);
            byte[] pdfBytes = _pdfExporter.ExportAmortization(rows);
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

    /// <summary>Switches the application dialog to the review stage.</summary>
    public void SwitchToReviewStage()
    {
        IsFormVisible = false;
        IsReviewVisible = true;
        DialogTitle = "Application Review";
        DialogActionText = "Submit";
    }

    /// <summary>Resets the application dialog state.</summary>
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
