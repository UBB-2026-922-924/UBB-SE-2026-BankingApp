namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Features.Savings.Dtos;
using Domain.Aggregates.SavingsAggregate;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Contracts.Features.Investments;
using Domain.Aggregates.SavingsAggregate.Entities;
using Infrastructure.Http.Features.Savings.Services;
using Shared.Enums;

/// <summary>
///     Coordinates savings account display and account workflows.
/// </summary>
public partial class SavingsViewModel : ObservableObject, IDisposable
{
    private const int InitialPage = 1;
    private const int DefaultTransactionPageSize = 10;
    private const int InitialAutoDepositDelayDays = 1;
    private const decimal ZeroAmount = 0m;
    private const string OneTimeFrequency = "OneTime";

    private readonly ISavingsPresentationRepoProxy _savingsPresentationRepoProxy;
    private readonly ISavingsRepoProxy _savingsRepoProxy;
    private readonly ISavingsUiRulesRepoProxy _savingsUiRulesRepoProxy;
    private readonly ISavingsWorkflowRepoProxy _savingsWorkflowRepoProxy;

    [ObservableProperty]
    public partial string AccountName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AutoDepositAmountText { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string AutoDepositFrequency { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool AutoDepositIsActive { get; set; } = true;
    [ObservableProperty]
    public partial string AutoDepositSaveMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial DateTimeOffset? AutoDepositStartDate { get; set; } = DateTimeOffset.Now.AddDays(InitialAutoDepositDelayDays);
    [ObservableProperty]
    public partial string BestInterestRate { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<SavingsAccount> CloseDestinationAccounts { get; set; } = new ObservableCollection<SavingsAccount>();

    [ObservableProperty]
    public partial string CloseResultMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool CloseSuccess { get; set; }

    private bool _closeUserConfirmed;

    private AutoDeposit? _currentAutoDeposit;

    [ObservableProperty]
    public partial int CurrentPage { get; set; } = InitialPage;

    [ObservableProperty]
    public partial string DepositAmountText { get; set; } = string.Empty;

    private CancellationTokenSource? _depositCancelationTokenSource;

    [ObservableProperty]
    public partial string DepositSource { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string DepositSuccessMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial ObservableCollection<FundingSourceOption> FundingSources { get; set; } = new ObservableCollection<FundingSourceOption>();
    [ObservableProperty]
    public partial bool HasExistingAutoDeposit { get; set; }
    [ObservableProperty]
    public partial string InitialDepositText { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string NumberOfAccountsText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(ShowAccountsList))]
    public partial ObservableCollection<SavingsAccount> SavingsAccounts { get; set; } = new ObservableCollection<SavingsAccount>();

    [ObservableProperty]
    public partial SavingsAccount? SelectedAccount { get; set; }

    private int _selectedCloseDestinationId;

    [ObservableProperty]
    public partial string SelectedFilter { get; set; } = "All";

    [ObservableProperty]
    public partial string SelectedFrequency { get; set; } = string.Empty;
    [ObservableProperty]
    public partial FundingSourceOption? SelectedFundingSource { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGoalSavings))]
    [NotifyPropertyChangedFor(nameof(IsFixedDeposit))]
    public partial string SelectedSavingsType { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ShowCreateConfirmation { get; set; }
    [ObservableProperty]
    public partial bool ShowDepositSuccess { get; set; }
    [ObservableProperty]
    public partial decimal? TargetAmount { get; set; }
    [ObservableProperty]
    public partial DateTimeOffset? TargetDate { get; set; }

    [ObservableProperty]
    public partial int TotalPages { get; set; }

    [ObservableProperty]
    public partial string TotalSavedAmount { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<SavingsTransaction> Transactions { get; set; } = new ObservableCollection<SavingsTransaction>();

    [ObservableProperty]
    public partial string WithdrawAmountText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial FundingSourceOption? WithdrawDestination { get; set; }
    [ObservableProperty]
    public partial string WithdrawResultMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool WithdrawSuccess { get; set; }
    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial int CurrentUserId { get; set; }
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string LivePreview { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool WithdrawHasEarlyRisk { get; set; }

    [ObservableProperty]
    public partial bool WithdrawHasPenalty { get; set; }

    [ObservableProperty]
    public partial decimal WithdrawEstimatedPenalty { get; set; }

    [ObservableProperty]
    public partial decimal WithdrawNetAmount { get; set; }

    [ObservableProperty]
    public partial string WithdrawPenaltyBreakdownText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string WithdrawNetAmountText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string WithdrawPenaltySummary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool CloseHasPenalty { get; set; }

    [ObservableProperty]
    public partial SavingsState State { get; set; } = SavingsState.Idle;

    private string _pendingTargetAmountText = string.Empty;

    private bool _isBusy;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SavingsViewModel" /> class.
    /// </summary>
    /// <param name="savingsRepoProxy">The savings HTTP proxy.</param>
    /// <param name="savingsWorkflowRepoProxy">The savings workflow proxy.</param>
    /// <param name="savingsUiRulesRepoProxy">The savings UI rules proxy.</param>
    /// <param name="savingsPresentationRepoProxy">The savings presentation proxy.</param>
    public SavingsViewModel(
        ISavingsRepoProxy savingsRepoProxy,
        ISavingsWorkflowRepoProxy savingsWorkflowRepoProxy,
        ISavingsUiRulesRepoProxy savingsUiRulesRepoProxy,
        ISavingsPresentationRepoProxy savingsPresentationRepoProxy)
    {
        this._savingsRepoProxy = savingsRepoProxy ?? throw new ArgumentNullException(nameof(savingsRepoProxy));
        this._savingsWorkflowRepoProxy = savingsWorkflowRepoProxy ?? throw new ArgumentNullException(nameof(savingsWorkflowRepoProxy));
        this._savingsUiRulesRepoProxy = savingsUiRulesRepoProxy ?? throw new ArgumentNullException(nameof(savingsUiRulesRepoProxy));
        this._savingsPresentationRepoProxy = savingsPresentationRepoProxy ?? throw new ArgumentNullException(nameof(savingsPresentationRepoProxy));
    }

    /// <summary>Gets a value indicating whether no savings accounts are loaded.</summary>
    public bool IsEmpty => !SavingsAccounts.Any();

    /// <summary>Gets a value indicating whether the account list should be shown.</summary>
    public bool ShowAccountsList => SavingsAccounts.Any();

    /// <summary>Gets a value indicating whether the selected account type is goal savings.</summary>
    public bool IsGoalSavings => SelectedSavingsType == "GoalSavings";

    /// <summary>Gets a value indicating whether the selected account type is fixed deposit.</summary>
    public bool IsFixedDeposit => SelectedSavingsType == "FixedDeposit";

    /// <summary>Gets validation errors keyed by form field.</summary>
    public Dictionary<string, string> FieldErrors { get; } = new Dictionary<string, string>();

    /// <summary>Gets the auto deposit setup label.</summary>
    public string ExistingLabel => HasExistingAutoDeposit ? "Modify" : "Set Up";

    /// <summary>Gets or sets the fixed deposit maturity date.</summary>
    public DateTimeOffset? MaturityDate { get; set; }

    /// <summary>Gets or sets the selected close destination account id.</summary>
    public int SelectedCloseDestinationId
    {
        get => _selectedCloseDestinationId;
        set
        {
            _selectedCloseDestinationId = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets a value indicating whether the user confirmed account closure.</summary>
    public bool CloseUserConfirmed
    {
        get => _closeUserConfirmed;
        set
        {
            _closeUserConfirmed = value;
            OnPropertyChanged();
        }
    }

    partial void OnSelectedAccountChanged(SavingsAccount? value)
    {
        _ = RefreshSelectedAccountDependentsAsync();
    }

    partial void OnDepositAmountTextChanged(string value)
    {
        _ = RefreshDepositPreviewAsync();
    }

    partial void OnWithdrawAmountTextChanged(string value)
    {
        _ = RefreshWithdrawPenaltyAsync();
    }

    partial void OnErrorMessageChanged(string value)
    {
        HasError = !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Refreshes all observable fields that depend on the currently selected account.
    /// Triggered via OnSelectedAccountChanged.
    /// </summary>
    private async Task RefreshSelectedAccountDependentsAsync()
    {
        if (_isBusy)
        {
            return;
        }
        if (SelectedAccount == null)
        {
            WithdrawHasEarlyRisk = false;
            WithdrawHasPenalty = false;
            WithdrawEstimatedPenalty = ZeroAmount;
            WithdrawNetAmount = ZeroAmount;
            WithdrawPenaltyBreakdownText = string.Empty;
            WithdrawNetAmountText = string.Empty;
            WithdrawPenaltySummary = string.Empty;
            CloseHasPenalty = false;
            LivePreview = string.Empty;
            return;
        }

        try
        {
            WithdrawHasEarlyRisk = SelectedAccount.MaturityDate.HasValue &&
                                        SelectedAccount.MaturityDate.Value.Date > DateTime.Today;
            CloseHasPenalty = await _savingsPresentationRepoProxy.CheckClosePenaltyRisk(SelectedAccount);

            if (WithdrawHasEarlyRisk)
            {
                decimal penaltyRate = await _savingsRepoProxy.GetPenaltyDecimalFor("EarlyWithdrawal");
                WithdrawPenaltySummary =
                    $"Early withdrawal penalty: {penaltyRate:P2} of amount. Maturity date: {SelectedAccount.MaturityDate:d}";
            }
            else
            {
                WithdrawPenaltySummary = string.Empty;
            }

            // Re-run amount-dependent refresh in case an amount was already typed.
            await RefreshWithdrawPenaltyAsync();
            await RefreshDepositPreviewAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>
    /// Refreshes the deposit live-preview label.
    /// Triggered via OnDepositAmountTextChanged and after account selection.
    /// </summary>
    private async Task RefreshDepositPreviewAsync()
    {
        if (_isBusy)
        {
            return;
        }
        if (SelectedAccount == null || string.IsNullOrWhiteSpace(DepositAmountText))
        {
            LivePreview = string.Empty;
            return;
        }

        try
        {
            LivePreview = await _savingsUiRulesRepoProxy.GetDepositPreview(
                DepositAmountText,
                SelectedAccount);
        }
        catch
        {
            LivePreview = string.Empty;
        }
    }

    /// <summary>
    /// Refreshes all withdraw penalty fields based on the current amount text.
    /// Triggered via OnWithdrawAmountTextChanged and after account selection.
    /// </summary>
    private async Task RefreshWithdrawPenaltyAsync()
    {
        if (_isBusy)
        {
            return;
        }
        if (!WithdrawHasEarlyRisk)
        {
            WithdrawEstimatedPenalty = ZeroAmount;
            WithdrawNetAmount = ZeroAmount;
            WithdrawHasPenalty = false;
            WithdrawPenaltyBreakdownText = string.Empty;
            WithdrawNetAmountText = string.Empty;
            return;
        }

        if (string.IsNullOrWhiteSpace(WithdrawAmountText))
        {
            WithdrawEstimatedPenalty = ZeroAmount;
            WithdrawNetAmount = ZeroAmount;
            WithdrawHasPenalty = false;
            WithdrawPenaltyBreakdownText = string.Empty;
            WithdrawNetAmountText = string.Empty;
            return;
        }

        decimal amount;
        try
        {
            amount = await _savingsUiRulesRepoProxy.ParsePositiveAmount(WithdrawAmountText);
        }
        catch (InvalidOperationException)
        {
            // Invalid/empty text — reset penalty fields silently.
            WithdrawEstimatedPenalty = ZeroAmount;
            WithdrawNetAmount = ZeroAmount;
            WithdrawHasPenalty = false;
            WithdrawPenaltyBreakdownText = string.Empty;
            WithdrawNetAmountText = string.Empty;
            return;
        }

        try
        {
            decimal penaltyRate = await _savingsRepoProxy.GetPenaltyDecimalFor("EarlyWithdrawal");
            WithdrawEstimatedPenalty = Math.Round(amount * penaltyRate, 2);
            WithdrawNetAmount = await _savingsUiRulesRepoProxy.GetWithdrawNetAmount(amount, WithdrawEstimatedPenalty);
            WithdrawHasPenalty = WithdrawEstimatedPenalty > ZeroAmount;
            WithdrawPenaltyBreakdownText = $"Penalty ({penaltyRate:P0}): -${WithdrawEstimatedPenalty:N2}";
            WithdrawNetAmountText = $"Net amount received: ${WithdrawNetAmount:N2}";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>Loads funding sources for the current user.</summary>
    public async Task LoadFundingSourcesAsync()
    {
        try
        {
            List<FundingSourceOption> fundingSourcesList = await _savingsRepoProxy.GetFundingSourcesAsync(CurrentUserId);
            FundingSources.Clear();
            foreach (FundingSourceOption fundingSource in fundingSourcesList)
            {
                FundingSources.Add(fundingSource);
            }

            SelectedFundingSource = await _savingsWorkflowRepoProxy.GetDefaultFundingSource(FundingSources);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    /// <summary>Copies create-account form values into the view model before submission.</summary>
    /// <param name="selectedSavingsType">The selected savings account type.</param>
    /// <param name="selectedFrequency">The selected deposit frequency.</param>
    /// <param name="accountName">The account name.</param>
    /// <param name="initialDepositText">The initial deposit text.</param>
    /// <param name="fundingSource">The selected funding source.</param>
    /// <param name="targetAmountText">The target amount text.</param>
    /// <param name="targetDate">The optional target date.</param>
    /// <param name="maturityDate">The optional maturity date.</param>
    public void PrepareCreateAccountSubmission(
        string selectedSavingsType,
        string selectedFrequency,
        string accountName,
        string initialDepositText,
        FundingSourceOption? fundingSource,
        string targetAmountText,
        DateTimeOffset? targetDate,
        DateTimeOffset? maturityDate)
    {
        SelectedSavingsType = selectedSavingsType;
        SelectedFrequency = string.IsNullOrWhiteSpace(selectedFrequency)
            ? OneTimeFrequency
            : selectedFrequency;
        AccountName = accountName;
        InitialDepositText = initialDepositText;
        SelectedFundingSource = fundingSource;
        TargetAmount = null;

        _pendingTargetAmountText = targetAmountText;

        TargetDate = IsGoalSavings ? targetDate : null;
        MaturityDate = SelectedSavingsType == "FixedDeposit" ? maturityDate : null;
    }

    private async Task<bool> ValidationCreateAccount()
    {
        if (string.IsNullOrWhiteSpace(SelectedFrequency))
        {
            SelectedFrequency = OneTimeFrequency;
        }

        if (IsGoalSavings && !string.IsNullOrWhiteSpace(_pendingTargetAmountText))
        {
            try
            {
                TargetAmount = await _savingsUiRulesRepoProxy
                    .ParsePositiveAmount(_pendingTargetAmountText);
            }
            catch (InvalidOperationException)
            {
                TargetAmount = null;
            }
        }

        Dictionary<string, string> errors = await _savingsUiRulesRepoProxy.ValidateCreateAccount(new ValidateCreateAccountRequest
        {
            SelectedSavingsType = SelectedSavingsType,
            AccountName = AccountName,
            InitialDepositText = InitialDepositText,
            HasFundingSource = SelectedFundingSource != null,
            SelectedFrequency = SelectedFrequency,
            TargetAmount = TargetAmount,
            TargetDate = TargetDate,
            IsGoalSavings = IsGoalSavings,
        });

        foreach (KeyValuePair<string, string> error in errors)
        {
            FieldErrors[error.Key] = error.Value;
        }

        OnPropertyChanged(nameof(FieldErrors));

        return FieldErrors.Count == 0;
    }

    private async Task ExecuteCreateAccountAsync()
    {
        IsLoading = true;
        try
        {
            decimal deposit = await _savingsUiRulesRepoProxy.ParsePositiveAmount(InitialDepositText);

            var createSavingsAccountDto = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = CurrentUserId,
                SavingsType = SelectedSavingsType,
                AccountName = AccountName.Trim(),
                InitialDeposit = deposit,
                FundingAccountId = SelectedFundingSource!.Id,
                TargetAmount = IsGoalSavings ? TargetAmount : null,
                TargetDate = IsGoalSavings ? TargetDate?.DateTime : null,
                MaturityDate = MaturityDate?.DateTime,
                DepositFrequency = string.IsNullOrWhiteSpace(SelectedFrequency) ||
                                   string.Equals(SelectedFrequency, OneTimeFrequency, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : await _savingsUiRulesRepoProxy.ParseDepositFrequency(SelectedFrequency),
            };

            await _savingsRepoProxy.CreateSavingsAccountAsync(createSavingsAccountDto, 0m);

            ShowCreateConfirmation = true;
            ResetCreateForm();
            await LoadAccountsAsync();
        }
        catch (Exception exception) when (
        exception is InvalidOperationException
        || exception is ArgumentException)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Creates a savings account from the current form values.</summary>
    [RelayCommand]
    public async Task CreateAccountAsync()
    {
        if (_isBusy)
        {
            return;
        }
        _isBusy = true;

        try
        {
            FieldErrors.Clear();
            ErrorMessage = string.Empty;
            ShowCreateConfirmation = false;

            if (!await ValidationCreateAccount())
            {
                return;
            }

            await ExecuteCreateAccountAsync();
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void ResetCreateForm()
    {
        AccountName = string.Empty;
        InitialDepositText = string.Empty;
        SelectedSavingsType = string.Empty;
        SelectedFrequency = OneTimeFrequency;
        TargetAmount = null;
        TargetDate = null;
        MaturityDate = null;
        _pendingTargetAmountText = string.Empty;
        FieldErrors.Clear();
    }

    /// <summary>Deposits funds into the selected savings account.</summary>
    [RelayCommand]
    public async Task DepositAsync()
    {
        if (_isBusy)
        {
            return;
        }
        _isBusy = true;

        try
        {
            ErrorMessage = string.Empty;
            ShowDepositSuccess = false;

            if (SelectedAccount == null)
            {
                ErrorMessage = "No account selected.";
                return;
            }

            decimal amount;
            try
            {
                amount = await _savingsUiRulesRepoProxy.ParsePositiveAmount(DepositAmountText);
            }
            catch
            {
                ErrorMessage = "Please enter a valid positive amount.";
                return;
            }

            _depositCancelationTokenSource?.Cancel();
            _depositCancelationTokenSource = new CancellationTokenSource();

            IsLoading = true;
            try
            {
                DepositResponseDto depositResponseDto = await _savingsRepoProxy.DepositAsync(
                    SelectedAccount.Id,
                    amount,
                    DepositSource);

                DepositSuccessMessage = $"Deposit successful! New balance: ${depositResponseDto.NewBalance:N2}";
                ShowDepositSuccess = true;
                DepositAmountText = string.Empty;
                await LoadAccountsAsync();
            }
            catch (InvalidOperationException exception)
            {
                ErrorMessage = exception.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    /// <summary>Cancels the in-flight deposit operation.</summary>
    public void CancelDeposit()
    {
        _depositCancelationTokenSource?.Cancel();
    }

    /// <summary>Loads savings accounts for the current user.</summary>
    [RelayCommand]
    public async Task LoadAccountsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            State = SavingsState.Loading;
            List<SavingsAccount> accountsList = await _savingsRepoProxy.GetSavingsAccountsByUserIdAsync(CurrentUserId);
            SavingsAccounts.Clear();
            foreach (SavingsAccount account in accountsList)
            {
                SavingsAccounts.Add(account);
            }

            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(ShowAccountsList));

            TotalSavedAmount = await _savingsPresentationRepoProxy.GetTotalSavedAmount(SavingsAccounts);
            NumberOfAccountsText =
                await _savingsPresentationRepoProxy.GetNumberOfAccountsText(SavingsAccounts.Count);
            BestInterestRate = await _savingsPresentationRepoProxy.GetBestInterestRate(SavingsAccounts);
            State = SavingsState.Ready;
        }
        catch (Exception exception) when (
        exception is ArgumentException
        || exception is InvalidOperationException)
        {
            ErrorMessage = exception.Message;
            State = SavingsState.Error;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Closes the specified savings account.</summary>
    /// <param name="account">The account to close.</param>
    [RelayCommand]
    public async Task CloseAccountAsync(SavingsAccount account)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            ClosureResultDto closureResultDto = await _savingsRepoProxy.CloseSavingsAccountAsync(
                account.Id,
                SelectedCloseDestinationId,
                0m,
                0m);
            bool ok = closureResultDto.Success;
            if (!ok)
            {
                ErrorMessage = "Failed to close account.";
                return;
            }

            await LoadAccountsAsync();
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

    /// <summary>Loads accounts that can receive funds from the account being closed.</summary>
    public async Task LoadCloseDestinationAccountsAsync()
    {
        CloseUserConfirmed = false;
        CloseResultMessage = string.Empty;
        CloseSuccess = false;
        List<SavingsAccount> openAccountsList = await _savingsRepoProxy.GetValidTransferDestinationsAsync(
            SelectedAccount!.Id,
            CurrentUserId);
        CloseDestinationAccounts.Clear();
        foreach (SavingsAccount account in openAccountsList)
        {
            CloseDestinationAccounts.Add(account);
        }

        SelectedCloseDestinationId =
            await _savingsWorkflowRepoProxy.GetDefaultCloseDestinationId(CloseDestinationAccounts);
        OnPropertyChanged(nameof(CloseHasPenalty));
    }

    /// <summary>Confirms and executes account closure.</summary>
    /// <returns>True when the account was closed successfully.</returns>
    public async Task<bool> ConfirmCloseAsync()
    {
        ValidationResponse closeValidation = await _savingsWorkflowRepoProxy.ValidateCloseConfirmation(
            CloseUserConfirmed,
            SelectedCloseDestinationId);
        if (!closeValidation.IsValid)
        {
            CloseResultMessage = closeValidation.ErrorMessage;
            return false;
        }

        IsLoading = true;
        try
        {
            ClosureResultDto result = await _savingsRepoProxy.CloseSavingsAccountAsync(
                SelectedAccount!.Id,
                SelectedCloseDestinationId,
                0m,
                0m);
            CloseSuccess = result.Success;
            CloseResultMessage = result.Success ? "Account closed successfully." : result.Message;
            if (result.Success)
            {
                await LoadAccountsAsync();
            }

            return result.Success;
        }
        catch (InvalidOperationException exception)
        {
            CloseResultMessage = exception.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<(bool IsValid, decimal Amount)> ValidateWithdrawInputAsync()
    {
        try
        {
            decimal amount = await _savingsUiRulesRepoProxy.ParsePositiveAmount(WithdrawAmountText);

            ValidationResponse validation = await _savingsWorkflowRepoProxy.ValidateWithdrawRequest(amount, WithdrawDestination);

            if (!validation.IsValid)
            {
                WithdrawResultMessage = validation.ErrorMessage;
                return (false, 0);
            }
            return (true, amount);
        }
        catch
        {
            WithdrawResultMessage = "Please enter a valid positive amount.";
            return (false, 0);
        }
    }

    private async Task<bool> RunWithdrawalTransactionAsync(decimal amount)
    {
        IsLoading = true;
        try
        {
            WithdrawResponseDto response = await _savingsRepoProxy.WithdrawAsync(
                SelectedAccount!.Id,
                amount,
                WithdrawDestination!.DisplayName,
                0m);
            WithdrawSuccess = response.Success;
            WithdrawResultMessage = await _savingsWorkflowRepoProxy.BuildWithdrawResultMessage(response);
            if (response.Success)
            {
                WithdrawAmountText = string.Empty;
                await LoadAccountsAsync();
            }
            return response.Success;
        }
        catch (ArgumentException exception)
        {
            WithdrawResultMessage = exception.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Confirms and executes a withdrawal from the selected account.</summary>
    /// <returns>True when the withdrawal completed successfully.</returns>
    public async Task<bool> ConfirmWithdrawAsync()
    {
        if (_isBusy)
        {
            return false;
        }
        _isBusy = true;
        try
        {
            WithdrawResultMessage = string.Empty;

            (bool isValid, decimal amount) = await ValidateWithdrawInputAsync();

            if (!isValid)
            {
                return false;
            }

            return await RunWithdrawalTransactionAsync(amount);
        }
        finally
        {
            _isBusy = false;
        }
    }

    /// <summary>Loads auto deposit settings for an account.</summary>
    /// <param name="accountId">The account id.</param>
    public async Task LoadAutoDepositAsync(int accountId)
    {
        AutoDepositSaveMessage = string.Empty;
        _currentAutoDeposit = await _savingsRepoProxy.GetAutoDepositAsync(accountId);
        if (_currentAutoDeposit != null)
        {
            HasExistingAutoDeposit = true;
            AutoDepositAmountText = _currentAutoDeposit.Amount.ToString(CultureInfo.InvariantCulture);
            AutoDepositFrequency = _currentAutoDeposit.Frequency.ToString();
            AutoDepositStartDate = new DateTimeOffset(_currentAutoDeposit.NextRunDate);
            AutoDepositIsActive = _currentAutoDeposit.IsActive;
        }
        else
        {
            HasExistingAutoDeposit = false;
            AutoDepositAmountText = string.Empty;
            AutoDepositFrequency = string.Empty;
            AutoDepositStartDate = DateTimeOffset.Now.AddDays(InitialAutoDepositDelayDays);
            AutoDepositIsActive = true;
        }
    }

    /// <summary>Saves auto deposit settings for the selected account.</summary>
    public async Task SaveAutoDepositAsync()
    {
        ErrorMessage = string.Empty;
        AutoDepositSaveMessage = string.Empty;

        decimal amount;
        try
        {
            amount = await _savingsUiRulesRepoProxy.ParsePositiveAmount(AutoDepositAmountText);
        }
        catch (InvalidOperationException)
        {
            ErrorMessage = "Auto deposit amount must be positive.";
            return;
        }

        if (string.IsNullOrWhiteSpace(AutoDepositFrequency))
        {
            ErrorMessage = "Please select a frequency.";
            return;
        }

        DepositFrequency frequency;
        try
        {
            frequency = await _savingsUiRulesRepoProxy.ParseDepositFrequency(AutoDepositFrequency);
        }
        catch (InvalidOperationException)
        {
            ErrorMessage = "Invalid frequency.";
            return;
        }

        var autoDeposit = AutoDeposit.Reconstitute(
            _currentAutoDeposit?.Id ?? 0,
            SelectedAccount!.Id,
            amount,
            frequency,
            AutoDepositStartDate?.DateTime ?? DateTime.Now.AddDays(InitialAutoDepositDelayDays),
            AutoDepositIsActive,
            SelectedAccount.FundingAccountId,
            dayOfMonth: null,
            dayOfWeek: null,
            updatedAt: null);

        await _savingsRepoProxy.SaveAutoDepositAsync(autoDeposit);
        AutoDepositSaveMessage = "Auto deposit saved successfully.";
        await LoadAutoDepositAsync(SelectedAccount.Id);
    }

    /// <summary>Loads savings transactions for an account.</summary>
    /// <param name="accountId">The account id.</param>
    public async Task LoadTransactionsAsync(int accountId)
    {
        try
        {
            GetTransactionsResponse result = await _savingsRepoProxy.GetTransactionsAsync(
                accountId,
                SelectedFilter,
                CurrentPage,
                DefaultTransactionPageSize);

            Transactions.Clear();

            foreach (SavingsTransaction transaction in result.Items)
            {
                Transactions.Add(transaction);
            }

            TotalPages = await _savingsUiRulesRepoProxy.GetTotalPages(result.TotalCount, DefaultTransactionPageSize);
        }
        catch (Exception exception) when (
        exception is InvalidOperationException
        || exception is ArgumentException)
        {
            ErrorMessage = exception.Message;
        }
    }

    /// <summary>Moves to the next transactions page.</summary>
    /// <param name="accountId">The account id.</param>
    public async Task NextPage(int accountId)
    {
        if (!await _savingsWorkflowRepoProxy.CanMoveToNextPage(CurrentPage, TotalPages))
        {
            return;
        }

        CurrentPage++;
        await LoadTransactionsAsync(accountId);
    }

    /// <summary>Moves to the previous transactions page.</summary>
    /// <param name="accountId">The account id.</param>
    public async Task PreviousPage(int accountId)
    {
        if (!await _savingsWorkflowRepoProxy.CanMoveToPreviousPage(CurrentPage))
        {
            return;
        }

        CurrentPage--;
        await LoadTransactionsAsync(accountId);
    }

    /// <summary>Changes the transactions filter.</summary>
    /// <param name="accountId">The account id.</param>
    /// <param name="filter">The filter value.</param>
    public async Task ChangeFilter(int accountId, string filter)
    {
        SelectedFilter = filter;
        CurrentPage = InitialPage;
        await LoadTransactionsAsync(accountId);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Transactions.Clear();

        SavingsAccounts.Clear();
        CloseDestinationAccounts.Clear();
        FundingSources.Clear();

        _depositCancelationTokenSource?.Cancel();
        _depositCancelationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}
