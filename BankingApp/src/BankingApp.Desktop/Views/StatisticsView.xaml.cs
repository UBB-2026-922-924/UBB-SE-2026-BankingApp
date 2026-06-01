namespace BankingApp.Desktop.Views;

using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class StatisticsView
{
    public StatisticsView(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
        Loaded += StatisticsView_Loaded;
        Unloaded += StatisticsView_Unloaded;
    }

    public StatisticsViewModel ViewModel { get; }

    private async void StatisticsView_Loaded(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.IsLoading && !ViewModel.HasData)
        {
            await ViewModel.LoadAsync();
        }
    }

    private void StatisticsView_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Dispose();
    }
}
