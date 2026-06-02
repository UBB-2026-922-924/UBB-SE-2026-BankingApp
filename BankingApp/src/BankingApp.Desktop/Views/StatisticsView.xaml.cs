namespace BankingApp.Desktop.Views;

using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>
///     Displays spending and balance statistics.
/// </summary>
public sealed partial class StatisticsView
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StatisticsView" /> class.
    /// </summary>
    /// <param name="viewModel">The statistics view model.</param>
    public StatisticsView(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
        Loaded += StatisticsView_Loaded;
        Unloaded += StatisticsView_Unloaded;
    }

    internal StatisticsViewModel ViewModel { get; }

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
