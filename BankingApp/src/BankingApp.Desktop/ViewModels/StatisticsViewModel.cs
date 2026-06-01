namespace BankingApp.Desktop.ViewModels;

using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using BankingApp.Application.Features.Authentication.Services;
using BankingApp.Contracts.Features.Statistics.Dtos;
using BankingApp.Contracts.Features.Statistics.Services;
using ErrorOr;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Shared;
using Shared.Commands;
using AsyncRelayCommand = CommunityToolkit.Mvvm.Input.AsyncRelayCommand;

public class StatisticsViewModel : ObservableObject
{
    private readonly IStatisticsService _statisticsService;
    private readonly IAuthService _authService;
    private readonly Shared.Commands.AsyncRelayCommand _refreshCommand;

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

    public StatisticsViewModel(IStatisticsService statisticsService, IAuthService authService)
    {
        _statisticsService = statisticsService;
        _authService = authService;
        SpendingByCategory = new ObservableCollection<CategorySpendingPointDto>();
        BalanceTrends = new ObservableCollection<BalanceTrendPointDto>();
        TopRecipients = new ObservableCollection<TopCounterpartyDto>();
        _refreshCommand = new Shared.Commands.AsyncRelayCommand(LoadAsync);
    }

    public ObservableCollection<CategorySpendingPointDto> SpendingByCategory { get; }

    public ObservableCollection<BalanceTrendPointDto> BalanceTrends { get; }

    public ObservableCollection<TopCounterpartyDto> TopRecipients { get; }

    public Shared.Commands.AsyncRelayCommand RefreshCommand => _refreshCommand;

    public new void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);
    }

    protected void SetState<T>(Observable<T> observable, T value)
    {
        observable.SetValue(value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                _refreshCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(LoadingVisibility));
            }
        }
    }

    public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsStatusOpen
    {
        get => _isStatusOpen;
        private set => SetProperty(ref _isStatusOpen, value);
    }

    public InfoBarSeverity StatusSeverity
    {
        get => _statusSeverity;
        private set => SetProperty(ref _statusSeverity, value);
    }

    public decimal Income
    {
        get => _income;
        private set => SetProperty(ref _income, value);
    }

    public decimal Expenses
    {
        get => _expenses;
        private set => SetProperty(ref _expenses, value);
    }

    public decimal Net
    {
        get => _net;
        private set => SetProperty(ref _net, value);
    }

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

    public string FormattedTotalSpendingLabel => $"Total spending: {TotalSpending:C2}";

    public double MaxCategoryAmount
    {
        get => _maxCategoryAmount;
        private set => SetProperty(ref _maxCategoryAmount, value);
    }

    public double MaxBalanceAmount
    {
        get => _maxBalanceAmount;
        private set => SetProperty(ref _maxBalanceAmount, value);
    }

    public double MaxTopRecipientAmount
    {
        get => _maxTopRecipientAmount;
        private set => SetProperty(ref _maxTopRecipientAmount, value);
    }

    public bool HasData => SpendingByCategory.Count > 0 || BalanceTrends.Count > 0 || TopRecipients.Count > 0;

    public async Task LoadAsync(object? redundant = null)
    {
        /* isn't it better to check in the server?
        if (!_authService.IsAuthenticated())
        {
            ShowStatus("You must sign in to view statistics.", InfoBarSeverity.Warning);
            return;
        }
        */

        try
        {
            IsLoading = true;
            ShowStatus(string.Empty, InfoBarSeverity.Informational);

            Task<ErrorOr<SpendingByCategoryResponse>> spendingTask = _statisticsService.GetSpendingByCategoryAsync();
            Task<ErrorOr<IncomeVsExpensesResponse>> incomeTask = _statisticsService.GetIncomeVsExpensesAsync();
            Task<ErrorOr<BalanceTrendsResponse>> balanceTask = _statisticsService.GetBalanceTrendsAsync();
            Task<ErrorOr<TopRecipientsResponse>> topRecipientsTask = _statisticsService.GetTopRecipientsAsync();

            await Task.WhenAll(spendingTask, incomeTask, balanceTask, topRecipientsTask);

            ErrorOr<SpendingByCategoryResponse> spendingResponse = await spendingTask;
            ErrorOr<IncomeVsExpensesResponse> incomeResponse = await incomeTask;
            ErrorOr<BalanceTrendsResponse> balanceResponse = await balanceTask;
            ErrorOr<TopRecipientsResponse> topRecipientsResponse = await topRecipientsTask;

            if (spendingResponse.Value?.Success != true ||
                incomeResponse.Value?.Success != true ||
                balanceResponse.Value?.Success != true ||
                topRecipientsResponse.Value?.Success != true)
            {
                ShowStatus("Failed to load one or more statistics sections.", InfoBarSeverity.Error);
                return;
            }

            ReplaceCollection(SpendingByCategory, spendingResponse.Value.Categories);
            ReplaceCollection(BalanceTrends, balanceResponse.Value.Points);
            ReplaceCollection(TopRecipients, topRecipientsResponse.Value.Recipients);

            TotalSpending = spendingResponse.Value.TotalSpending;
            Income = incomeResponse.Value.Income;
            Expenses = incomeResponse.Value.Expenses;
            Net = incomeResponse.Value.Net;
            MaxCategoryAmount = SpendingByCategory.Count == 0 ? 1 : (double)SpendingByCategory.Max(item => item.Amount);
            MaxBalanceAmount = BalanceTrends.Count == 0 ? 1 : (double)BalanceTrends.Max(item => item.Balance);
            MaxTopRecipientAmount = TopRecipients.Count == 0 ? 1 : (double)TopRecipients.Max(item => item.TotalAmount);

            ShowStatus("Statistics refreshed successfully.", InfoBarSeverity.Success);
            OnPropertyChanged(nameof(HasData));
        }
        catch (UnauthorizedAccessException)
        {
            ShowStatus("Your session expired. Please sign in again.", InfoBarSeverity.Warning);
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to load statistics: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasData));
        }
    }

    private void ShowStatus(string message, InfoBarSeverity severity)
    {
        StatusMessage = message;
        StatusSeverity = severity;
        IsStatusOpen = !string.IsNullOrWhiteSpace(message);
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, System.Collections.Generic.IEnumerable<T> source)
    {
        target.Clear();
        foreach (T item in source)
        {
            target.Add(item);
        }
    }
}
