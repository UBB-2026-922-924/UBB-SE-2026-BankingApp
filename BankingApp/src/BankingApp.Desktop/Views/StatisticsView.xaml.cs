namespace BankingApp.Desktop.Views;

using BankingApp.Desktop;
using BankingApp.Desktop.ViewModels;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class StatisticsView : Page
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StatisticsView"/> class.
    /// </summary>
    public StatisticsView(StatisticsViewModel statisticsViewModel)
    {
        InitializeComponent();
        ViewModel = statisticsViewModel;
        DataContext = statisticsViewModel;
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

    }
}

