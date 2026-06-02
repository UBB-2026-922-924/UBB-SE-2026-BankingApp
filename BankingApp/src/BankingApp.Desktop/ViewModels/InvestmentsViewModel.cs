namespace BankingApp.Desktop.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Contracts.Http;
using Domain.Aggregates.InvestmentAggregate;
using Domain.Aggregates.InvestmentAggregate.Entities;
using Infrastructure.Http.Features.Investments.Services;
using Navigation;
using Shared.Enums;

/// <summary>
///     Coordinates investment portfolio display and filtering.
/// </summary>
public partial class InvestmentsViewModel : ObservableObject
{
    private readonly IInvestmentsRepoProxy _investmentsRepoProxy;
    private readonly IAppNavigationService _navigationService;
    private bool _hasLoaded;

    /// <summary>Gets or sets the selected holding filter.</summary>
    [ObservableProperty]
    public partial string ActiveFilterType { get; set; } = "All";

    /// <summary>Gets or sets the holdings displayed after filtering.</summary>
    [ObservableProperty]
    public partial ObservableCollection<InvestmentHolding> DisplayedHoldings { get; set; } = [];

    /// <summary>Gets or sets a value indicating whether portfolio data is loading.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyStateVisible))]
    [NotifyPropertyChangedFor(nameof(IsHoldingsVisible))]
    public partial bool IsPortfolioLoading { get; set; }

    /// <summary>Gets or sets the current user portfolio.</summary>
    [ObservableProperty]
    public partial Portfolio UserPortfolio { get; set; } = Portfolio.Create(0);

    /// <summary>Gets or sets the current investments state.</summary>
    [ObservableProperty]
    public partial InvestmentsState State { get; set; } = InvestmentsState.Idle;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvestmentsViewModel" /> class.
    /// </summary>
    /// <param name="investmentsRepoProxy">The investments HTTP proxy.</param>
    /// <param name="navigationService">The application navigation service.</param>
    public InvestmentsViewModel(
        IInvestmentsRepoProxy investmentsRepoProxy,
        IAppNavigationService navigationService)
    {
        this._investmentsRepoProxy = investmentsRepoProxy;
        this._navigationService = navigationService;
        SelectFilterCommand = new RelayCommand<string>(ApplyFilter);
        OpenTradeDialogCommand = new RelayCommand(HandleTrade);
    }

    /// <summary>Gets the command that selects a holding filter.</summary>
    public ICommand SelectFilterCommand { get; }

    /// <summary>Gets the command that opens the trade workflow.</summary>
    public ICommand OpenTradeDialogCommand { get; }

    /// <summary>Gets a value indicating whether the empty state should be shown.</summary>
    public bool IsEmptyStateVisible => !IsPortfolioLoading && !DisplayedHoldings.Any();

    /// <summary>Gets a value indicating whether holdings should be shown.</summary>
    public bool IsHoldingsVisible => !IsEmptyStateVisible;

    /// <summary>Loads portfolio data once for the current view instance.</summary>
    public void EnsureInitialized()
    {
        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;
        LoadUserPortfolio();
    }

    /// <summary>Loads the user's portfolio from the HTTP API.</summary>
    public async void LoadUserPortfolio()
    {
        IsPortfolioLoading = true;
        State = InvestmentsState.Loading;
        try
        {
            Portfolio? portfolio = await _investmentsRepoProxy.GetAsync<Portfolio>(ApiEndpoints.Investments.PortfolioFull);

            if (portfolio != null)
            {
                UserPortfolio = portfolio;
                RefreshDisplayedHoldings();
            }

            State = InvestmentsState.Ready;
        }
        catch (Exception exception)
        {
            State = InvestmentsState.Error;
            Debug.WriteLine($"LoadUserPortfolio API error: {exception.Message}");
        }
        finally
        {
            IsPortfolioLoading = false;
        }
    }

    /// <summary>Applies a holding filter.</summary>
    /// <param name="filterType">The filter value.</param>
    public void ApplyFilter(string? filterType)
    {
        ActiveFilterType = string.IsNullOrWhiteSpace(filterType) ? "All" : filterType;
    }

    /// <summary>Stops investment market data polling.</summary>
    public void StopMarketDataPolling()
    {
    }

    partial void OnActiveFilterTypeChanged(string value)
    {
        RefreshDisplayedHoldings();
    }

    private void HandleTrade()
    {
        _navigationService.NavigateToContent<Views.CryptoTradingView>();
    }

    private void RefreshDisplayedHoldings()
    {
        DisplayedHoldings.Clear();
        IEnumerable<InvestmentHolding> holdings = UserPortfolio?.Holdings ?? Enumerable.Empty<InvestmentHolding>();

        IEnumerable<InvestmentHolding> filtered = ActiveFilterType switch
        {
            "All" => holdings,
            "Stocks" => holdings.Where(h => h.AssetType.Equals("Stock", StringComparison.OrdinalIgnoreCase)),
            "Crypto" => holdings.Where(h => h.AssetType.Equals("Crypto", StringComparison.OrdinalIgnoreCase)),
            _ => holdings.Where(h => h.AssetType.Equals(ActiveFilterType, StringComparison.OrdinalIgnoreCase))
        };

        foreach (InvestmentHolding holding in filtered)
        {
            DisplayedHoldings.Add(holding);
        }

        NotifyEmptyStateChanged();
    }

    private void NotifyEmptyStateChanged()
    {
        OnPropertyChanged(nameof(IsEmptyStateVisible));
        OnPropertyChanged(nameof(IsHoldingsVisible));
    }
}
