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

public partial class InvestmentsViewModel : ObservableObject
{
    private readonly IInvestmentsRepoProxy _investmentsRepoProxy;
    private readonly IAppNavigationService _navigationService;
    private bool _hasLoaded;

    [ObservableProperty]
    private string _activeFilterType = "All";

    [ObservableProperty]
    private ObservableCollection<InvestmentHolding> _displayedHoldings = new ObservableCollection<InvestmentHolding>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyStateVisible))]
    [NotifyPropertyChangedFor(nameof(IsHoldingsVisible))]
    private bool _isPortfolioLoading;

    [ObservableProperty]
    private Portfolio _userPortfolio = new Portfolio();

    [ObservableProperty]
    private InvestmentsState _state = InvestmentsState.Idle;

    public InvestmentsViewModel(
        IInvestmentsRepoProxy investmentsRepoProxy,
        IAppNavigationService navigationService)
    {
        this._investmentsRepoProxy = investmentsRepoProxy;
        this._navigationService = navigationService;
        SelectFilterCommand = new RelayCommand<string>(ApplyFilter);
        OpenTradeDialogCommand = new RelayCommand(HandleTrade);
    }

    public ICommand SelectFilterCommand { get; }

    public ICommand OpenTradeDialogCommand { get; }

    public bool IsEmptyStateVisible => !IsPortfolioLoading && !DisplayedHoldings.Any();

    public bool IsHoldingsVisible => !IsEmptyStateVisible;

    public void EnsureInitialized()
    {
        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;
        LoadUserPortfolio();
    }

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

    public void ApplyFilter(string? filterType)
    {
        ActiveFilterType = string.IsNullOrWhiteSpace(filterType) ? "All" : filterType;
    }

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
