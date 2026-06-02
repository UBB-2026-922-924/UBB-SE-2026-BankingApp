namespace BankingApp.Desktop.Views;

using ViewModels;
using Microsoft.UI.Xaml;

/// <summary>
///     Hosts the multistep transfer wizard.
///     The view receives its <see cref="TransferViewModel" /> through constructor injection
///     and triggers account loading when the page is displayed.
/// </summary>
public sealed partial class TransferView
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives the transfer wizard.</param>
    public TransferView(TransferViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
        Loaded += OnPageLoaded;
    }

    /// <summary>
    ///     Gets the view model bound to this view.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TransferViewModel ViewModel { get; }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        _ = ViewModel.LoadAccountsAsync();
    }
}
