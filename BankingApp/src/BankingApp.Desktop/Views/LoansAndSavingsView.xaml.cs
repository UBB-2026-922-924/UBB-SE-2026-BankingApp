namespace BankingApp.Desktop.Views;

using Microsoft.UI.Xaml.Controls;
using BankingApp.Desktop.ViewModels;

public sealed partial class LoansAndSavingsView : Page
{
    public LoansAndSavingsView()
    {
        this.InitializeComponent();
        ViewModel = new LoansAndSavingsViewModel(
            new SavingsViewModel(App.SavingsService),
            new LoansViewModel(App.LoansService));
    }

    public LoansAndSavingsViewModel ViewModel
    {
        get => (LoansAndSavingsViewModel)this.DataContext;
        set => this.DataContext = value;
    }
}