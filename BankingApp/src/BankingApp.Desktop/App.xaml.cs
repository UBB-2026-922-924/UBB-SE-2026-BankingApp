namespace BankingApp.Desktop;

using System;
using Configuration;
using DependencyInjection;
using Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Navigation;
using Serilog;
using Views;

/// <summary>
///     Application entry point and composition root for the desktop client.
/// </summary>
/// <remarks>
///     Startup infrastructure is configured here before the main window is created.
/// </remarks>
public partial class App
{
    private Window? _window;

    /// <summary>
    ///     Initializes the desktop application infrastructure.
    /// </summary>
    public App()
    {
        AppLogging.Configure();
        GlobalExceptionLogging.Register(this);

        Log.Information(
            "Desktop app starting. BaseDirectory={BaseDirectory}, LogDirectory={LogDirectory}.",
            AppContext.BaseDirectory,
            AppLogging.LogDirectory);

        IConfigurationRoot configuration = AppConfiguration.Build();

        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddSerilog(dispose: true));
        services.AddClientServices(configuration);

        Services = services.BuildServiceProvider();

        InitializeComponent();
    }

    /// <summary>
    ///     Gets the root service provider for the application.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    ///     Handles application launch by creating and activating the main window.
    /// </summary>
    /// <param name="args">
    ///     Launch information provided by WinUI.
    /// </param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Log.Information("Desktop app launched.");

        IAppNavigationService navigationService = Services.GetRequiredService<IAppNavigationService>();

        _window = new MainWindow(navigationService);
        _window.Activate();
    }
}
