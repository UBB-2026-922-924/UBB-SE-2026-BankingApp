namespace BankingApp.Desktop.Views;

using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>
///     Displays investment portfolio holdings.
/// </summary>
public sealed partial class InvestmentsView : Page
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvestmentsView" /> class.
    /// </summary>
    /// <param name="viewModel">The investments view model.</param>
    public InvestmentsView(InvestmentsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = ViewModel;

        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
    }

    internal InvestmentsViewModel ViewModel { get; }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.EnsureInitialized();
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.StopMarketDataPolling();
        Loaded -= OnPageLoaded;
        Unloaded -= OnPageUnloaded;
    }
}
