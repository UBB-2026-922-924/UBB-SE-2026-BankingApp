namespace BankingApp.Desktop.Views;

using Microsoft.UI.Xaml.Controls;
using ViewModels;

/// <summary>
///     Hosts the loans and savings workspace.
/// </summary>
public sealed partial class LoansAndSavingsView : Page
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LoansAndSavingsView" /> class.
    /// </summary>
    /// <param name="viewModel">The combined loans and savings view model.</param>
    /// <param name="savingsView">The savings child view.</param>
    /// <param name="loansView">The loans child view.</param>
    public LoansAndSavingsView(
        LoansAndSavingsViewModel viewModel,
        SavingsView savingsView,
        LoansView loansView)
    {
        InitializeComponent();
        ViewModel = viewModel;
        savingsView.DataContext = ViewModel.SavingsVm;
        loansView.DataContext = ViewModel.LoansVm;
        SavingsContent.Content = savingsView;
        LoansContent.Content = loansView;
    }

    internal LoansAndSavingsViewModel ViewModel
    {
        get => (LoansAndSavingsViewModel)DataContext;
        set => DataContext = value;
    }
}
