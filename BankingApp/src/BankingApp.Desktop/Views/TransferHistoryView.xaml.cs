namespace BankingApp.Desktop.Views;

using ViewModels;
using Microsoft.UI.Xaml;

/// <summary>
///     Displays the authenticated user's past transfers as a scrollable list.
///     Data is loaded automatically when the page is first shown and can be
///     refreshed via the Refresh button.
/// </summary>
public sealed partial class TransferHistoryView
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferHistoryView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model injected by the DI container.</param>
    public TransferHistoryView(TransferHistoryViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        Loaded += OnPageLoaded;
    }

    /// <summary>
    ///     Gets the view model backing this page.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TransferHistoryViewModel ViewModel { get; }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.LoadHistoryAsync();
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.LoadHistoryAsync();
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }
}
