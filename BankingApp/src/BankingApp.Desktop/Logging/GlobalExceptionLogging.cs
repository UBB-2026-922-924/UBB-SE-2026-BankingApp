namespace BankingApp.Desktop.Logging;

using System;
using System.Threading.Tasks;
using Serilog;
using XamlUnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

internal static class GlobalExceptionLogging
{
    public static void Register(App app)
    {
        app.UnhandledException += OnXamlUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private static void OnXamlUnhandledException(object _, XamlUnhandledExceptionEventArgs eventArgs)
    {
        Log.Fatal(eventArgs.Exception, "Unhandled exception on the XAML UI thread.");
        Log.CloseAndFlush();
    }

    private static void OnAppDomainUnhandledException(object? _, UnhandledExceptionEventArgs eventArgs)
    {
        if (eventArgs.ExceptionObject is Exception exception)
        {
            Log.Fatal(
                exception,
                "Unhandled exception in the current AppDomain. IsTerminating={IsTerminating}.",
                eventArgs.IsTerminating);
        }
        else
        {
            Log.Fatal(
                "Unhandled non-Exception object in the current AppDomain. IsTerminating={IsTerminating}. ExceptionObject={ExceptionObject}.",
                eventArgs.IsTerminating,
                eventArgs.ExceptionObject);
        }

        Log.CloseAndFlush();
    }

    private static void OnUnobservedTaskException(object? _, UnobservedTaskExceptionEventArgs eventArgs)
    {
        Log.Error(eventArgs.Exception, "Unobserved task exception.");
        Log.CloseAndFlush();
    }
}
