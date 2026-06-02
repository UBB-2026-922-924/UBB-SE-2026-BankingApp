namespace BankingApp.Desktop.ViewModels;

using System.Diagnostics;
using Contracts.Features.Investments.Dtos;
using Contracts.Http;
using Domain.Aggregates.InvestmentAggregate;
using Infrastructure.Http.Features.Investments.Services;
using Navigation;
using Session;
using Shared.Enums;

/// <summary>
///     Coordinates the crypto trading workflow.
/// </summary>
public partial class CryptoTradingViewModel : ObservableObject
{
    private readonly IAuthenticationSession _authenticationSession;
    private readonly IInvestmentsRepoProxy _investmentsRepoProxy;
    private readonly IAppNavigationService _navigationService;

    /// <summary>Gets or sets the selected asset ticker.</summary>
    [ObservableProperty]
    public partial string SelectedTicker { get; set; } = "BTC";

    /// <summary>Gets or sets the selected trade action.</summary>
    [ObservableProperty]
    public partial string ActionType { get; set; } = "BUY";

    /// <summary>Gets or sets the entered quantity text.</summary>
    [ObservableProperty]
    public partial string QuantityText { get; set; } = "0";

    /// <summary>Gets or sets the current portfolio balance.</summary>
    [ObservableProperty]
    public partial decimal CurrentBalance { get; set; }

    /// <summary>Gets or sets the status message displayed to the user.</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    /// <summary>Gets or sets the estimated trade fee.</summary>
    [ObservableProperty]
    public partial decimal EstimatedFee { get; set; }

    /// <summary>Gets or sets the estimated total trade amount.</summary>
    [ObservableProperty]
    public partial decimal TotalAmount { get; set; }

    /// <summary>Gets or sets a value indicating whether a trade is processing.</summary>
    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    /// <summary>Gets or sets the current investments state.</summary>
    [ObservableProperty]
    public partial InvestmentsState State { get; set; } = InvestmentsState.Idle;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CryptoTradingViewModel" /> class.
    /// </summary>
    /// <param name="authenticationSession">The current authentication session.</param>
    /// <param name="investmentsRepoProxy">The investments HTTP proxy.</param>
    /// <param name="navigationService">The application navigation service.</param>
    public CryptoTradingViewModel(
        IAuthenticationSession authenticationSession,
        IInvestmentsRepoProxy investmentsRepoProxy,
        IAppNavigationService navigationService)
    {
        this._authenticationSession = authenticationSession;
        this._investmentsRepoProxy = investmentsRepoProxy;
        this._navigationService = navigationService;
        _ = LoadBalance();
    }

    /// <summary>Navigates back to the investments view.</summary>
    public void NavigateBackToInvestments()
    {
        _navigationService.NavigateToContent<Views.InvestmentsView>();
    }

    partial void OnSelectedTickerChanged(string value) => CalculateLiveTotals();

    partial void OnQuantityTextChanged(string value) => CalculateLiveTotals();

    /// <summary>Executes the selected trade.</summary>
    [RelayCommand]
    public async Task ExecuteTradeAsync()
    {
        if (!decimal.TryParse(QuantityText, out decimal qty) || qty <= 0)
        {
            StatusMessage = "Please insert a valid currency volume.";
            return;
        }

        if (ActionType == "BUY" && TotalAmount > CurrentBalance)
        {
            StatusMessage = $" Insufficient Funds: Total cost ({TotalAmount:N2} RON) exceeds your wallet balance ({CurrentBalance:N2} RON).";
            return;
        }

        IsProcessing = true;
        State = InvestmentsState.Loading;
        StatusMessage = "Processing secure network order verification...";

        try
        {
            if (!_authenticationSession.CurrentUserId.HasValue)
            {
                StatusMessage = " Session expired. Please log in again.";
                State = InvestmentsState.Error;
                return;
            }

            decimal currentMarketPrice = GetCurrentMarketPrice(SelectedTicker);
            var request = new ExecuteTradeRequest
            {
                Ticker = SelectedTicker,
                ActionType = ActionType,
                Quantity = qty,
                PricePerUnit = currentMarketPrice,
                Fees = EstimatedFee,
            };

            await _investmentsRepoProxy.PostAsync<ExecuteTradeRequest, object>(
                ApiEndpoints.Investments.TradeFull,
                request);

            StatusMessage = "Transaction verified successfully!";
            State = InvestmentsState.Ready;
            NavigateBackToInvestments();
        }
        catch (Exception ex)
        {
            State = InvestmentsState.Error;
            StatusMessage = ParseTradeError(ex.Message);
            Debug.WriteLine($"Trade Failure Trace: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task LoadBalance()
    {
        try
        {
            Portfolio? portfolio = await _investmentsRepoProxy.GetAsync<Portfolio>(ApiEndpoints.Investments.PortfolioFull);
            CurrentBalance = portfolio?.TotalValue ?? 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed loading portfolio balance: {ex.Message}");
        }
    }

    private void CalculateLiveTotals()
    {
        if (!decimal.TryParse(QuantityText, out decimal qty) || qty <= 0)
        {
            EstimatedFee = 0;
            TotalAmount = 0;
            StatusMessage = string.Empty;
            return;
        }

        decimal currentMarketPrice = GetCurrentMarketPrice(SelectedTicker);
        decimal principalCost = qty * currentMarketPrice;
        EstimatedFee = Math.Round(principalCost * 0.015m, 2);
        TotalAmount = Math.Round(principalCost + EstimatedFee, 2);
        StatusMessage = $"Ready to submit trade at {currentMarketPrice:N2} RON unit valuation.";
    }

    private static decimal GetCurrentMarketPrice(string ticker)
    {
        return ticker switch
        {
            "BTC" => 65000.00m,
            "ETH" => 2550.00m,
            "SOL" => 145.00m,
            _ => 0m
        };
    }

    private static string ParseTradeError(string rawError)
    {
        if (!rawError.Contains("Body: ", StringComparison.Ordinal))
        {
            return $" Connection Error: {rawError}";
        }

        try
        {
            int bodyStartIndex = rawError.IndexOf("Body: ", StringComparison.Ordinal) + 6;
            string jsonBody = rawError[bodyStartIndex..];

            if (!jsonBody.Contains("\"detail\":\"", StringComparison.Ordinal))
            {
                return " Transaction rejected by server validations.";
            }

            int detailStart = jsonBody.IndexOf("\"detail\":\"", StringComparison.Ordinal) + 10;
            int detailEnd = jsonBody.IndexOf('"', detailStart);
            return $" {jsonBody[detailStart..detailEnd]}";
        }
        catch
        {
            return " Validation processing failure.";
        }
    }
}
