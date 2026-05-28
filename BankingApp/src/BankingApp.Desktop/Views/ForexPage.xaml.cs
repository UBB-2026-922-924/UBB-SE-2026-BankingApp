namespace BankingApp.Desktop.Views;

using System;
using ViewModels;
using Microsoft.UI.Xaml;

/// <summary>
///     Code-behind for the FX currency exchange page.
/// </summary>
public sealed partial class ForexPage
{
    private readonly ForexViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexPage"/> class.
    /// </summary>
    /// <param name="viewModel">The exchange view model injected by DI.</param>
    public ForexPage(ForexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += ForexPage_Loaded;
    }

    private async void ForexPage_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAccountsAsync();
        await _viewModel.LoadHistoryAsync();
    }
}
