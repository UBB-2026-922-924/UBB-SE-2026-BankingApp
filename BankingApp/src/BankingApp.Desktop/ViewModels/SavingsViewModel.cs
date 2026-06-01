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
    private string _accountName = string.Empty;

    [ObservableProperty]
    private string _autoDepositAmountText = string.Empty;
    [ObservableProperty]
    private string _autoDepositFrequency = string.Empty;
    [ObservableProperty]
    private bool _autoDepositIsActive = true;
    [ObservableProperty]
    private string _autoDepositSaveMessage = string.Empty;
    [ObservableProperty]
    private DateTimeOffset? _autoDepositStartDate = DateTimeOffset.Now.AddDays(InitialAutoDepositDelayDays);
    [ObservableProperty]
    private string _bestInterestRate = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SavingsAccount> _closeDestinationAccounts = new ObservableCollection<SavingsAccount>();

    [ObservableProperty]
    private string _closeResultMessage = string.Empty;
    [ObservableProperty]
    private bool _closeSuccess;

    private bool _closeUserConfirmed;

    private AutoDeposit? _currentAutoDeposit;

    [ObservableProperty]
    private int _currentPage = InitialPage;

    [ObservableProperty]
    private string _depositAmountText = string.Empty;

    private CancellationTokenSource? _depositCancelationTokenSource;

    [ObservableProperty]
    private string _depositSource = string.Empty;
    [ObservableProperty]
    private string _depositSuccessMessage = string.Empty;
    [ObservableProperty]
    private ObservableCollection<FundingSourceOption> _fundingSources = new ObservableCollection<FundingSourceOption>();
    [ObservableProperty]
    private bool _hasExistingAutoDeposit;
    [ObservableProperty]
    private string _initialDepositText = string.Empty;
    [ObservableProperty]
    private string _numberOfAccountsText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(ShowAccountsList))]
    private ObservableCollection<SavingsAccount> _savingsAccounts = new ObservableCollection<SavingsAccount>();

    [ObservableProperty]
    private SavingsAccount? _selectedAccount;

    private int _selectedCloseDestinationId;

    [ObservableProperty] private string _selectedFilter = "All";

    [ObservableProperty] private string _selectedFrequency = string.Empty;
    [ObservableProperty] private FundingSourceOption? _selectedFundingSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGoalSavings))]
    [NotifyPropertyChangedFor(nameof(IsFixedDeposit))]
    private string _selectedSavingsType = string.Empty;

    [ObservableProperty]
    private bool _showCreateConfirmation;
    [ObservableProperty]
    private bool _showDepositSuccess;
    [ObservableProperty]
    private decimal? _targetAmount;
    [ObservableProperty]
    private DateTimeOffset? _targetDate;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private string _totalSavedAmount = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SavingsTransaction> _transactions = new ObservableCollection<SavingsTransaction>();

    [ObservableProperty]
    private string _withdrawAmountText = string.Empty;

    [ObservableProperty]
    private FundingSourceOption? _withdrawDestination;
    [ObservableProperty]
    private string _withdrawResultMessage = string.Empty;
    [ObservableProperty]
    private bool _withdrawSuccess;
    [ObservableProperty]
    private bool _isLoading;
    [ObservableProperty]
    private User _currentUser;
    [ObservableProperty]
    private string _errorMessage;
    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _livePreview = string.Empty;

    [ObservableProperty]
    private bool _withdrawHasEarlyRisk;

    [ObservableProperty]
    private bool _withdrawHasPenalty;

    [ObservableProperty]
    private decimal _withdrawEstimatedPenalty;

    [ObservableProperty]
    private decimal _withdrawNetAmount;

    [ObservableProperty]
    private string _withdrawPenaltyBreakdownText = string.Empty;

    [ObservableProperty]
    private string _withdrawNetAmountText = string.Empty;

    [ObservableProperty]
    private string _withdrawPenaltySummary = string.Empty;

    [ObservableProperty]
    private bool _closeHasPenalty;

    [ObservableProperty]
    private SavingsState _state = SavingsState.Idle;

    private string _pendingTargetAmountText = string.Empty;

    private bool _isBusy;

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

    public bool IsEmpty => !SavingsAccounts.Any();

    public bool ShowAccountsList => SavingsAccounts.Any();

    public bool IsGoalSavings => SelectedSavingsType == "GoalSavings";

    public bool IsFixedDeposit => SelectedSavingsType == "FixedDeposit";

    public Dictionary<string, string> FieldErrors { get; } = new Dictionary<string, string>();

    public string ExistingLabel => HasExistingAutoDeposit ? "Modify" : "Set Up";

    public DateTimeOffset? MaturityDate { get; set; }

    public int SelectedCloseDestinationId
    {
        get => _selectedCloseDestinationId;
        set
        {
            _selectedCloseDestinationId = value;
            OnPropertyChanged();
        }
    }

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

    public async Task LoadFundingSourcesAsync()
    {
        try
        {
            List<FundingSourceOption> fundingSourcesList = await _savingsRepoProxy.GetFundingSourcesAsync(CurrentUser.Id);
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

        return !FieldErrors.Any();
    }

    private async Task ExecuteCreateAccountAsync()
    {
        IsLoading = true;
        try
        {
            decimal deposit = await _savingsUiRulesRepoProxy.ParsePositiveAmount(InitialDepositText);

            var createSavingsAccountDto = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = CurrentUser.Id,
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
                    SelectedAccount.IdentificationNumber,
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

    public void CancelDeposit()
    {
        _depositCancelationTokenSource?.Cancel();
    }

    [RelayCommand]
    public async Task LoadAccountsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            State = SavingsState.Loading;
            List<SavingsAccount> accountsList = await _savingsRepoProxy.GetSavingsAccountsByUserIdAsync(CurrentUser.Id);
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

    [RelayCommand]
    public async Task CloseAccountAsync(SavingsAccount account)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            ClosureResultDto closureResultDto = await _savingsRepoProxy.CloseSavingsAccountAsync(
                account.IdentificationNumber,
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

    public async Task LoadCloseDestinationAccountsAsync()
    {
        CloseUserConfirmed = false;
        CloseResultMessage = string.Empty;
        CloseSuccess = false;
        List<SavingsAccount> openAccountsList = await _savingsRepoProxy.GetValidTransferDestinationsAsync(
            SelectedAccount!.IdentificationNumber,
            CurrentUser.Id);
        CloseDestinationAccounts.Clear();
        foreach (SavingsAccount account in openAccountsList)
        {
            CloseDestinationAccounts.Add(account);
        }

        SelectedCloseDestinationId =
            await _savingsWorkflowRepoProxy.GetDefaultCloseDestinationId(CloseDestinationAccounts);
        OnPropertyChanged(nameof(CloseHasPenalty));
    }

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
                SelectedAccount!.IdentificationNumber,
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
                SelectedAccount!.IdentificationNumber,
                amount,
                WithdrawDestination.DisplayName,
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

        var autoDeposit = new AutoDeposit
        {
            Id = _currentAutoDeposit?.Id ?? default,
            SavingsAccountId = SelectedAccount!.IdentificationNumber,
            Amount = amount,
            Frequency = frequency,
            NextRunDate = AutoDepositStartDate?.DateTime ?? DateTime.Now.AddDays(InitialAutoDepositDelayDays),
            IsActive = AutoDepositIsActive,
            SourceAccountId = SelectedAccount.FundingAccount?.Id,
        };

        await _savingsRepoProxy.SaveAutoDepositAsync(autoDeposit);
        AutoDepositSaveMessage = "Auto deposit saved successfully.";
        await LoadAutoDepositAsync(SelectedAccount.IdentificationNumber);
    }

    public async Task LoadTransactionsAsync(int accountId)
    {
        try
        {
            GetTransactionsResponse result = await _savingsRepoProxy.GetTransactionsAsync(
                accountId,
                _selectedFilter,
                _currentPage,
                DefaultTransactionPageSize);

            _transactions.Clear();

            foreach (SavingsTransaction transaction in result.Items)
            {
                _transactions.Add(transaction);
            }

            _totalPages = await _savingsUiRulesRepoProxy.GetTotalPages(result.TotalCount, DefaultTransactionPageSize);
        }
        catch (Exception exception) when (
        exception is InvalidOperationException
        || exception is ArgumentException)
        {
            ErrorMessage = exception.Message;
        }
    }

    public async Task NextPage(int accountId)
    {
        if (!await _savingsWorkflowRepoProxy.CanMoveToNextPage(_currentPage, _totalPages))
        {
            return;
        }

        _currentPage++;
        await LoadTransactionsAsync(accountId);
    }

    public async Task PreviousPage(int accountId)
    {
        if (!await _savingsWorkflowRepoProxy.CanMoveToPreviousPage(_currentPage))
        {
            return;
        }

        _currentPage--;
        await LoadTransactionsAsync(accountId);
    }

    public async Task ChangeFilter(int accountId, string filter)
    {
        _selectedFilter = filter;
        _currentPage = InitialPage;
        await LoadTransactionsAsync(accountId);
    }

    public void Dispose()
    {
        _transactions.Clear();

        SavingsAccounts.Clear();
        CloseDestinationAccounts.Clear();
        FundingSources.Clear();

        _depositCancelationTokenSource?.Cancel();
        _depositCancelationTokenSource?.Dispose();
    }
}
