namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BankingApp.Contracts.Features.BillPayments.Dtos;
using BankingApp.Contracts.Features.Forex;
using BankingApp.Contracts.Features.Forex.Dtos;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.Forex.Services;
using ErrorOr;
using Logging;
using Microsoft.Extensions.Logging;
using Session;
using Shared;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>Manages exchange-rate preview and foreign-exchange execution for the desktop client.</summary>
public partial class ForexViewModel : ObservableObject
{
    private const int InitialStep = 1;
    private const int PreviewStep = 2;
    private const int ResultStep = 4;
    private const decimal MinimumAmount = 0m;

    private readonly IAuthenticationSession _authenticationSession;
    private readonly IBillPaymentService _billPaymentService;
    private readonly IForexService _forexService;
    private readonly ILogger<ForexViewModel> _logger;

    // Not observable — derived from AmountText via OnAmountTextChanged
    private decimal _amount;

    /// <summary>Initializes a new instance of the <see cref="ForexViewModel"/> class.</summary>
    public ForexViewModel(
        IAuthenticationSession authenticationSession,
        IBillPaymentService billPaymentService,
        IForexService forexService,
        ILogger<ForexViewModel> logger)
    {
        _authenticationSession = authenticationSession ?? throw new ArgumentNullException(nameof(authenticationSession));
        _billPaymentService = billPaymentService ?? throw new ArgumentNullException(nameof(billPaymentService));
        _forexService = forexService ?? throw new ArgumentNullException(nameof(forexService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CurrentStep = InitialStep;
        AvailableCurrencies = new ObservableCollection<string>(ForexConstants.SupportedCurrencies);
        Accounts = [];
        TransactionHistory = [];
    }

    /// <summary>Gets the currencies available for exchange.</summary>
    public ObservableCollection<string> AvailableCurrencies { get; }

    /// <summary>Gets the user's active accounts.</summary>
    public ObservableCollection<AccountDto> Accounts { get; }

    /// <summary>Gets the user's exchange transaction history.</summary>
    public ObservableCollection<ForexTransactionResponse> TransactionHistory { get; }

    /// <summary>Gets or sets the current step in the exchange flow.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInitialStep))]
    [NotifyPropertyChangedFor(nameof(IsPreviewStep))]
    [NotifyPropertyChangedFor(nameof(IsResultStep))]
    public partial int CurrentStep { get; set; } = default!;

    /// <summary>Gets a value indicating whether the current step is the initial step.</summary>
    public bool IsInitialStep => CurrentStep == InitialStep;

    /// <summary>Gets a value indicating whether the current step is the preview step.</summary>
    public bool IsPreviewStep => CurrentStep == PreviewStep;

    /// <summary>Gets a value indicating whether the current step is the result step.</summary>
    public bool IsResultStep => CurrentStep == ResultStep;

    /// <summary>Gets or sets a value indicating whether the exchange history is visible.</summary>
    [ObservableProperty]
    public partial bool IsHistoryVisible { get; set; } = false;

    /// <summary>Gets or sets the selected source account.</summary>
    [ObservableProperty]
    public partial AccountDto? SelectedSourceAccount { get; set; }

    /// <summary>Gets or sets the selected target account.</summary>
    [ObservableProperty]
    public partial AccountDto? SelectedTargetAccount { get; set; }

    /// <summary>Gets or sets the currency being sold.</summary>
    [ObservableProperty]
    public partial string SourceCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the currency being bought.</summary>
    [ObservableProperty]
    public partial string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the source amount as entered by the user.</summary>
    [ObservableProperty]
    public partial string AmountText { get; set; } = string.Empty;

    /// <summary>Gets the parsed source amount.</summary>
    public decimal Amount => _amount;

    /// <summary>Gets or sets the live exchange rate shown in the preview.</summary>
    [ObservableProperty]
    public partial decimal LiveRate { get; set; } = default!;

    /// <summary>Gets or sets the commission shown in the preview.</summary>
    [ObservableProperty]
    public partial decimal Commission { get; set; } = default!;

    /// <summary>Gets or sets the target amount shown in the preview.</summary>
    [ObservableProperty]
    public partial decimal TargetAmount { get; set; } = default!;

    /// <summary>Gets or sets the reference of the completed exchange transaction.</summary>
    [ObservableProperty]
    public partial string TransactionReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the current user-facing error message.</summary>
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the exchange flow is busy.</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; } = default!;

    partial void OnAmountTextChanged(string value)
    {
        _amount = decimal.TryParse(value, out decimal parsed) ? parsed : MinimumAmount;
    }

    /// <summary>Loads the user's accounts from the API.</summary>
    public async Task LoadAccountsAsync()
    {
        try
        {
            ErrorOr<List<AccountDto>> result = await _billPaymentService.GetAccountsAsync();
            if (result.IsError)
            {
                ErrorMessage = UserMessages.Transfer.AccountLoadFailed;
                return;
            }

            Accounts.Clear();
            foreach (AccountDto account in result.Value)
            {
                Accounts.Add(account);
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    /// <summary>Loads the user's exchange history from the API.</summary>
    public async Task LoadHistoryAsync()
    {
        try
        {
            ErrorOr<List<ForexTransactionResponse>> result = await _forexService.GetHistoryAsync();
            if (result.IsError)
            {
                ErrorMessage = UserMessages.Exchange.HistoryLoadFailed;
                return;
            }

            TransactionHistory.Clear();
            foreach (ForexTransactionResponse transaction in result.Value)
            {
                TransactionHistory.Add(transaction);
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    /// <summary>Loads an exchange preview for the current currencies and amount.</summary>
    [RelayCommand]
    public async Task LoadPreviewAsync()
    {
        ErrorMessage = string.Empty;

        if (SelectedSourceAccount == null || SelectedTargetAccount == null)
        {
            ErrorMessage = UserMessages.Exchange.AccountRequired;
            return;
        }

        if (string.IsNullOrWhiteSpace(SourceCurrency) || string.IsNullOrWhiteSpace(TargetCurrency))
        {
            ErrorMessage = UserMessages.Exchange.CurrencyRequired;
            return;
        }

        if (SourceCurrency == TargetCurrency)
        {
            ErrorMessage = UserMessages.Exchange.SameCurrency;
            return;
        }

        if (_amount <= MinimumAmount)
        {
            ErrorMessage = UserMessages.Exchange.AmountRequired;
            return;
        }

        IsLoading = true;
        try
        {
            ErrorOr<ForexRatePreviewResponse> result =
                await _forexService.GetPreviewAsync(SourceCurrency, TargetCurrency, _amount);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.Exchange.PreviewFailed;
                DesktopLogMessages.RatePreviewFailed(_logger, result.Errors);
                return;
            }

            ForexRatePreviewResponse preview = result.Value;
            LiveRate = preview.ExchangeRate;
            Commission = preview.Commission;
            TargetAmount = preview.TargetAmount;
            CurrentStep = PreviewStep;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            DesktopLogMessages.RatePreviewFailedUnexpected(_logger, exception);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Executes the exchange using the values currently shown in the flow.</summary>
    [RelayCommand]
    public async Task ExecuteExchangeAsync()
    {
        ErrorMessage = string.Empty;

        if (SelectedSourceAccount == null || SelectedTargetAccount == null)
        {
            ErrorMessage = UserMessages.Exchange.AccountRequired;
            return;
        }

        if (_amount <= MinimumAmount)
        {
            ErrorMessage = UserMessages.Exchange.AmountRequired;
            return;
        }

        IsLoading = true;
        try
        {
            var request = new ForexTransactionRequest
            {
                UserId = _authenticationSession.CurrentUserId ?? 0,
                SourceAccountId = SelectedSourceAccount.Id,
                TargetAccountId = SelectedTargetAccount.Id,
                SourceCurrency = SourceCurrency,
                TargetCurrency = TargetCurrency,
                SourceAmount = _amount,
            };

            ErrorOr<ForexTransactionResponse> result = await _forexService.ExecuteAsync(request);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.Exchange.ExecuteFailed;
                DesktopLogMessages.ExchangeExecutionFailed(_logger, result.Errors);
                return;
            }

            TransactionReference = $"TX-{result.Value.Id}";
            CurrentStep = ResultStep;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            DesktopLogMessages.ExchangeExecutionFailedUnexpected(_logger, exception);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Resets the exchange flow back to its initial state.</summary>
    [RelayCommand]
    public void Reset()
    {
        SelectedSourceAccount = null;
        SelectedTargetAccount = null;
        SourceCurrency = string.Empty;
        TargetCurrency = string.Empty;
        AmountText = string.Empty;
        LiveRate = 0;
        Commission = 0;
        TargetAmount = 0;
        TransactionReference = string.Empty;
        ErrorMessage = string.Empty;
        CurrentStep = InitialStep;
    }

    /// <summary>Toggles the visibility of the exchange history.</summary>
    [RelayCommand]
    public void ToggleHistory()
    {
        IsHistoryVisible = !IsHistoryVisible;
    }
}
