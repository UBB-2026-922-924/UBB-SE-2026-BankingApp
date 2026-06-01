namespace BankingApp.Desktop.Views;

using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class InvestmentsView : Page
{
    public InvestmentsView(InvestmentsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = ViewModel;

        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
    }

    public InvestmentsViewModel ViewModel { get; }

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
