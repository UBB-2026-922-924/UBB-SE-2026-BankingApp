namespace BankingApp.Desktop.ViewModels;

using System.Diagnostics;
using Contracts.Features.Investments.Dtos;
using Contracts.Http;
using Domain.Aggregates.InvestmentAggregate;
using Infrastructure.Http.Features.Investments.Services;
using Navigation;
using Session;
using Shared.Enums;

public partial class CryptoTradingViewModel : ObservableObject
{
    private readonly IAuthenticationSession _authenticationSession;
    private readonly IInvestmentsRepoProxy _investmentsRepoProxy;
    private readonly IAppNavigationService _navigationService;

    [ObservableProperty]
    private string _selectedTicker = "BTC";

    [ObservableProperty]
    private string _actionType = "BUY";

    [ObservableProperty]
    private string _quantityText = "0";

    [ObservableProperty]
    private decimal _currentBalance;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private decimal _estimatedFee;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private InvestmentsState _state = InvestmentsState.Idle;

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

    public void NavigateBackToInvestments()
    {
        _navigationService.NavigateToContent<Views.InvestmentsView>();
    }

    partial void OnSelectedTickerChanged(string value) => CalculateLiveTotals();

    partial void OnQuantityTextChanged(string value) => CalculateLiveTotals();

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
