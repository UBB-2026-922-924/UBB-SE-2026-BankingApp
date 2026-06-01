namespace BankingApp.Desktop.Views;

using Microsoft.UI.Xaml.Controls;
using ViewModels;

public sealed partial class LoansAndSavingsView : Page
{
    public LoansAndSavingsView(LoansAndSavingsViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    public LoansAndSavingsViewModel ViewModel
    {
        get => (LoansAndSavingsViewModel)DataContext;
        set => DataContext = value;
    }
}
