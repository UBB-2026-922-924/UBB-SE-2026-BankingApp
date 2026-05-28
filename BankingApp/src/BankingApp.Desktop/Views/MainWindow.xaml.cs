namespace BankingApp.Desktop.Views;

using Navigation;

/// <summary>
///     Hosts the application's root frame and initializes the first navigated view.
/// </summary>
public sealed partial class MainWindow
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MainWindow" /> class,
    ///     wires the navigation controls and sets the current page to the <see cref="LoginView" />.
    /// </summary>
    /// <param name="navigationService">
    ///     Provides page navigation controls to the <see cref="RootFrame" />.
    /// </param>
    /// <returns>The result of the operation.</returns>
    public MainWindow(IAppNavigationService navigationService)
    {
        InitializeComponent();
        navigationService.SetFrame(RootFrame);
        navigationService.NavigateTo<LoginView>();
    }
}
