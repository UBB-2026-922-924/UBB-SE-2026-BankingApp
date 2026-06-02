namespace BankingApp.Desktop.Navigation;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

/// <summary>
///     Provides navigation services for the application, enabling switching between pages and managing navigation
///     history.
/// </summary>
/// <remarks>
///     Pages are resolved through the DI container so that their constructor dependencies
///     (e.g. view models) are injected automatically. Navigation is performed by setting
///     the Frame.Content property directly rather than calling <see cref="Frame.Navigate(Type)" />,
///     because the latter instantiates pages via reflection and bypasses the container.
/// </remarks>
public class AppNavigationService : IAppNavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private Frame? _contentFrame;
    private Frame? _frame;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AppNavigationService" /> class.
    /// </summary>
    /// <param name="serviceProvider">
    ///     The DI container used to resolve page instances with their injected dependencies.
    /// </param>
    /// <returns>The result of the operation.</returns>
    public AppNavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    /// <param name="frame">The newFrame value.</param>
    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    /// <inheritdoc />
    /// <param name="frame">The newFrame value.</param>
    public void SetContentFrame(Frame frame)
    {
        _contentFrame = frame;
    }

    /// <inheritdoc />
    public void NavigateTo<TPage>()
        where TPage : class
    {
        TPage page = _serviceProvider.GetRequiredService<TPage>();
        _frame!.Content = page;
    }

    /// <inheritdoc />
    public void NavigateToContent<TPage>()
        where TPage : class
    {
        TPage page = _serviceProvider.GetRequiredService<TPage>();
        _contentFrame!.Content = page;
    }

    /// <inheritdoc />
    public void NavigateToContent<TPage>(Action<TPage> configure)
        where TPage : class
    {
        TPage page = _serviceProvider.GetRequiredService<TPage>();
        configure(page);
        _contentFrame!.Content = page;
    }
}
