namespace BankingApp.Desktop.Views;

using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class CryptoTradingView : Page
{
    public CryptoTradingViewModel ViewModel { get; }

    public CryptoTradingView(CryptoTradingViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = ViewModel;
    }

    private void OnActionTypeChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && ViewModel != null)
        {
            ViewModel.ActionType = rb.Tag?.ToString() ?? "BUY";
        }
    }

    private void OnBackButtonClicked(object sender, RoutedEventArgs e)
    {
        // Reliable UI-level frame manipulation routing
        if (Frame != null && Frame.CanGoBack)
        {
            Frame.GoBack();
        }
        else
        {
            ViewModel.NavigateBackToInvestments();
        }
    }
}
