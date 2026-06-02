namespace BankingApp.Desktop.ViewModels;

using System.Collections.ObjectModel;
using Contracts.Features.Statistics.Dtos;
using Infrastructure.Http.Features.Statistics.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Session;
using Shared.Enums;

/// <summary>
///     Coordinates statistics retrieval and presentation state.
/// </summary>
public partial class StatisticsViewModel : ObservableObject, IDisposable
{
    private readonly IAuthenticationSession _authenticationSession;
    private readonly AsyncRelayCommand _refreshCommand;
    private readonly IStatisticsRepoProxy _statisticsRepoProxy;

    private bool _isLoading;
    private string _statusMessage = string.Empty;
    private bool _isStatusOpen;
    private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;
    private decimal _income;
    private decimal _expenses;
    private decimal _net;
    private decimal _totalSpending;
    private double _maxCategoryAmount = 1;
    private double _maxBalanceAmount = 1;
    private double _maxTopRecipientAmount = 1;

    /// <summary>Gets or sets the current statistics state.</summary>
    [ObservableProperty]
    public partial StatisticsState State { get; set; } = StatisticsState.Idle;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StatisticsViewModel" /> class.
    /// </summary>
    /// <param name="statisticsRepoProxy">The statistics HTTP proxy.</param>
    /// <param name="authenticationSession">The current authentication session.</param>
    public StatisticsViewModel(
        IStatisticsRepoProxy statisticsRepoProxy,
        IAuthenticationSession authenticationSession)
    {
        this._statisticsRepoProxy = statisticsRepoProxy;
        this._authenticationSession = authenticationSession;
        SpendingByCategory = new ObservableCollection<CategorySpendingPointDto>();
        BalanceTrends = new ObservableCollection<BalanceTrendPointDto>();
        TopRecipients = new ObservableCollection<TopCounterpartyDto>();
        _refreshCommand = new AsyncRelayCommand(LoadAsync, () => !_isLoading);
    }

    /// <summary>Gets spending grouped by category.</summary>
    public ObservableCollection<CategorySpendingPointDto> SpendingByCategory { get; }

    /// <summary>Gets balance trend points.</summary>
    public ObservableCollection<BalanceTrendPointDto> BalanceTrends { get; }

    /// <summary>Gets the top recipient or merchant counters.</summary>
    public ObservableCollection<TopCounterpartyDto> TopRecipients { get; }

    /// <summary>Gets the refresh command.</summary>
    public AsyncRelayCommand RefreshCommand => _refreshCommand;

    /// <summary>Gets a value indicating whether statistics are loading.</summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                _refreshCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(LoadingVisibility));
            }
        }
    }

    /// <summary>Gets the loading overlay visibility.</summary>
    public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Gets the status message.</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>Gets a value indicating whether the status bar is open.</summary>
    public bool IsStatusOpen
    {
        get => _isStatusOpen;
        private set => SetProperty(ref _isStatusOpen, value);
    }

    /// <summary>Gets the status severity.</summary>
    public InfoBarSeverity StatusSeverity
    {
        get => _statusSeverity;
        private set => SetProperty(ref _statusSeverity, value);
    }

    /// <summary>Gets the income total.</summary>
    public decimal Income
    {
        get => _income;
        private set => SetProperty(ref _income, value);
    }

    /// <summary>Gets the expenses total.</summary>
    public decimal Expenses
    {
        get => _expenses;
        private set => SetProperty(ref _expenses, value);
    }

    /// <summary>Gets the net income total.</summary>
    public decimal Net
    {
        get => _net;
        private set => SetProperty(ref _net, value);
    }

    /// <summary>Gets the total spending amount.</summary>
    public decimal TotalSpending
    {
        get => _totalSpending;
        private set
        {
            if (SetProperty(ref _totalSpending, value))
            {
                OnPropertyChanged(nameof(FormattedTotalSpendingLabel));
            }
        }
    }

    /// <summary>Gets the formatted total spending label.</summary>
    public string FormattedTotalSpendingLabel => $"Total spending: {TotalSpending:C2}";

    /// <summary>Gets the maximum category amount for chart scaling.</summary>
    public double MaxCategoryAmount
    {
        get => _maxCategoryAmount;
        private set => SetProperty(ref _maxCategoryAmount, value);
    }

    /// <summary>Gets the maximum balance amount for chart scaling.</summary>
    public double MaxBalanceAmount
    {
        get => _maxBalanceAmount;
        private set => SetProperty(ref _maxBalanceAmount, value);
    }

    /// <summary>Gets the maximum top recipient amount for chart scaling.</summary>
    public double MaxTopRecipientAmount
    {
        get => _maxTopRecipientAmount;
        private set => SetProperty(ref _maxTopRecipientAmount, value);
    }

    /// <summary>Gets a value indicating whether any statistics data is available.</summary>
    public bool HasData => SpendingByCategory.Count > 0 || BalanceTrends.Count > 0 || TopRecipients.Count > 0;

    /// <summary>Loads all statistics sections.</summary>
    public async Task LoadAsync()
    {
        if (!_authenticationSession.CurrentUserId.HasValue)
        {
            State = StatisticsState.Error;
            ShowStatus("You must sign in to view statistics.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            State = StatisticsState.Loading;
            ShowStatus(string.Empty, InfoBarSeverity.Informational);

            Task<SpendingByCategoryResponse> spendingTask = _statisticsRepoProxy.GetSpendingByCategoryAsync();
            Task<IncomeVsExpensesResponse> incomeTask = _statisticsRepoProxy.GetIncomeVsExpensesAsync();
            Task<BalanceTrendsResponse> balanceTask = _statisticsRepoProxy.GetBalanceTrendsAsync();
            Task<TopRecipientsResponse> topRecipientsTask = _statisticsRepoProxy.GetTopRecipientsAsync();

            await Task.WhenAll(spendingTask, incomeTask, balanceTask, topRecipientsTask);

            SpendingByCategoryResponse spendingResponse = await spendingTask;
            IncomeVsExpensesResponse incomeResponse = await incomeTask;
            BalanceTrendsResponse balanceResponse = await balanceTask;
            TopRecipientsResponse topRecipientsResponse = await topRecipientsTask;

            if (!spendingResponse.Success ||
                !incomeResponse.Success ||
                !balanceResponse.Success ||
                !topRecipientsResponse.Success)
            {
                State = StatisticsState.Error;
                ShowStatus("Failed to load one or more statistics sections.", InfoBarSeverity.Error);
                return;
            }

            ReplaceCollection(SpendingByCategory, spendingResponse.Categories);
            ReplaceCollection(BalanceTrends, balanceResponse.Points);
            ReplaceCollection(TopRecipients, topRecipientsResponse.Recipients);

            TotalSpending = spendingResponse.TotalSpending;
            Income = incomeResponse.Income;
            Expenses = incomeResponse.Expenses;
            Net = incomeResponse.Net;
            MaxCategoryAmount = SpendingByCategory.Count == 0 ? 1 : (double)SpendingByCategory.Max(item => item.Amount);
            MaxBalanceAmount = BalanceTrends.Count == 0 ? 1 : (double)BalanceTrends.Max(item => item.Balance);
            MaxTopRecipientAmount = TopRecipients.Count == 0 ? 1 : (double)TopRecipients.Max(item => item.TotalAmount);

            State = StatisticsState.Ready;
            ShowStatus("Statistics refreshed successfully.", InfoBarSeverity.Success);
            OnPropertyChanged(nameof(HasData));
        }
        catch (UnauthorizedAccessException)
        {
            State = StatisticsState.Error;
            ShowStatus("Your session expired. Please sign in again.", InfoBarSeverity.Warning);
        }
        catch (Exception ex)
        {
            State = StatisticsState.Error;
            ShowStatus($"Failed to load statistics: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasData));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (T item in source)
        {
            target.Add(item);
        }
    }

    private void ShowStatus(string message, InfoBarSeverity severity)
    {
        StatusMessage = message;
        StatusSeverity = severity;
        IsStatusOpen = !string.IsNullOrWhiteSpace(message);
    }
}
