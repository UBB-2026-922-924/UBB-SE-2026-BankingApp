namespace BankingApp.Desktop.Views;

using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>
///     Displays the crypto trading workflow.
/// </summary>
public sealed partial class CryptoTradingView : Page
{
    internal CryptoTradingViewModel ViewModel { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CryptoTradingView" /> class.
    /// </summary>
    /// <param name="viewModel">The crypto trading view model.</param>
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
