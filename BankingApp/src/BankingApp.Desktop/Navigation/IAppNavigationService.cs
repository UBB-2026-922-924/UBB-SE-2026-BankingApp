namespace BankingApp.Desktop.Navigation;

using Microsoft.UI.Xaml.Controls;

/// <summary>
///     Navigation service interface for the application,
///     defining methods for setting navigation frames.
/// </summary>
public interface IAppNavigationService
{
    /// <summary>
    ///     Sets the main navigation frame for the application,
    ///     which will be used for primary page navigation.
    /// </summary>
    /// <param name="frame">The frame to use for primary navigation.</param>
    public void SetFrame(Frame frame);

    /// <summary>
    ///     Sets the content frame used to display navigation content,
    ///     allowing for separation of main navigation and content display.
    /// </summary>
    /// <param name="frame">The frame to use for content navigation.</param>
    public void SetContentFrame(Frame frame);

    /// <summary>
    ///     Navigates to a specified page type within the main navigation frame.
    ///     The page is resolved from the DI container so its constructor dependencies are injected.
    /// </summary>
    /// <typeparam name="TPage">The page type to navigate to. Must be a reference type registered in the container.</typeparam>
    public void NavigateTo<TPage>()
        where TPage : class;

    /// <summary>
    ///     Navigates to a specified page type within the content frame.
    ///     The page is resolved from the DI container so its constructor dependencies are injected.
    /// </summary>
    /// <typeparam name="TPage">The page type to navigate to. Must be a reference type registered in the container.</typeparam>
    public void NavigateToContent<TPage>()
        where TPage : class;
}
