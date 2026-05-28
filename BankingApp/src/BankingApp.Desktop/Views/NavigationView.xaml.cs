namespace BankingApp.Desktop.Views;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Navigation;
using Session;

/// <summary>
///     Hosts the application shell after login: renders the sidebar and manages the inner content frame
///     where feature pages (Dashboard, Profile, etc.) are displayed.
/// </summary>
public sealed partial class NavigationView
{
    private const int MaximumInlineNotificationBadgeCount = 99;
    private const string OverflowNotificationBadgeText = "99+";
    private readonly IAuthenticationSession _authenticationSession;
    private readonly List<Button> _navButtons;
    private readonly IAppNavigationService _navigationService;

    /// <summary>Initializes a new instance of the <see cref="NavigationView"/> class.</summary>
    public NavigationView(IAuthenticationSession authenticationSession, IAppNavigationService navigationService)
    {
        InitializeComponent();
        Current = this;
        _navButtons =
        [
            NavDashboard, NavTransfers, NavBillPayments, NavCards,
            NavTransferHistory, NavCurrencyExchange, NavSavings,
            NavInvestments, NavStatistics, NavSupport, NavProfile,
            NavBeneficiaries
        ];
        _authenticationSession = authenticationSession;
        _navigationService = navigationService;
        _navigationService.SetContentFrame(ContentFrame);
        _navigationService.NavigateToContent<DashboardView>();
    }

    /// <summary>Gets the current shell instance.</summary>
    public static NavigationView? Current { get; private set; }

    /// <summary>Updates the notification badge count shown in the shell.</summary>
    public void UpdateNotificationBadge(int count)
    {
        if (count <= 0)
        {
            NotificationBadge.Visibility = Visibility.Collapsed;
            return;
        }

        NotificationBadgeText.Text = count > MaximumInlineNotificationBadgeCount
            ? OverflowNotificationBadgeText
            : count.ToString(CultureInfo.InvariantCulture);
        NotificationBadge.Visibility = Visibility.Visible;
    }

    /// <summary>Displays the placeholder dialog for a not-yet-implemented feature.</summary>
    public async Task ShowComingSoonAsync(string feature)
    {
        var dialog = new ContentDialog
        {
            Title = feature,
            Content = $"{feature} is coming soon.",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }

    /// <summary>Navigates to the transfer screen and marks it active in the shell.</summary>
    public void NavigateToTransfers()
    {
        SetActiveNav(NavTransfers);
        _navigationService.NavigateToContent<TransferView>();
    }

/// <summary>Navigates to the currency exchange screen and marks it active in the shell.</summary>
public void NavigateToCurrencyExchange()
{
    SetActiveNav(NavCurrencyExchange);
    _navigationService.NavigateToContent<ForexPage>();
}

/// <summary>Navigates to the bill payments screen and marks it active in the shell.</summary>
public void NavigateToBillPayments()
{
    SetActiveNav(NavBillPayments);
    _navigationService.NavigateToContent<BillPayView>();
}

    private void SetActiveNav(Button selected)
    {
        foreach (Button button in _navButtons)
        {
            button.Style = (Style)Resources["NavItemStyle"];
        }

        selected.Style = (Style)Resources["NavItemActiveStyle"];
    }

    private void NavDashboard_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavDashboard);
        _navigationService.NavigateToContent<DashboardView>();
    }

    private void NavProfile_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavProfile);
        _navigationService.NavigateToContent<ProfileView>();
    }

    private void NavBeneficiaries_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavBeneficiaries);
        _navigationService.NavigateToContent<BeneficiariesView>();
    }

    private void NavTransfers_Click(object sender, RoutedEventArgs e)
    {
        NavigateToTransfers();
    }

    private void NavBillPayments_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavBillPayments);
        _navigationService.NavigateToContent<BillPayView>();
    }

    private void NavCards_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavCards);
        _navigationService.NavigateToContent<CardsView>();
    }

    private void NavTransferHistory_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavTransferHistory);
        _navigationService.NavigateToContent<TransferHistoryView>();
    }

    private void NavCurrencyExchange_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavCurrencyExchange);
        _navigationService.NavigateToContent<ForexPage>();
    }

    private async void NavSavings_Click(object sender, RoutedEventArgs e)
    {
        try { await ShowComingSoonAsync("Savings & Loans"); }
        catch
        { /* Dialog display failure is non-critical. */ }
    }

    private async void NavInvestments_Click(object sender, RoutedEventArgs e)
    {
        try { await ShowComingSoonAsync("Investments & Trading"); }
        catch
        { /* Dialog display failure is non-critical. */ }
    }

    private async void NavStatistics_Click(object sender, RoutedEventArgs e)
    {
        try { await ShowComingSoonAsync("Statistics"); }
        catch
        { /* Dialog display failure is non-critical. */ }
    }

    private async void NavSupport_Click(object sender, RoutedEventArgs e)
    {
        try { await ShowComingSoonAsync("Support"); }
        catch
        { /* Dialog display failure is non-critical. */ }
    }

    private void NotificationBell_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        string message = NotificationBadge.Visibility == Visibility.Visible
            ? $"You have {NotificationBadgeText.Text} unread notifications."
            : "You have no unread notifications.";
        _ = ShowAlertAsync("Notifications", message);
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _authenticationSession.Clear();
        }
        catch
        {
            // ignored
        }

        _navigationService.NavigateTo<LoginView>();
    }

    private async Task ShowAlertAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }
}
