namespace BankingApp.Desktop.Logging;

using System;
using System.Globalization;
using System.IO;
using Serilog;

internal static class AppLogging
{
    private const int RetainedLoggingFileCountLimit = 14;
    private const string LoggingFileFormat = "bankingapp-client-.log";

    public static string LogDirectory { get; private set; } = string.Empty;

    public static void Configure()
    {
        LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BankingApp",
            "Logs");

        Directory.CreateDirectory(LogDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                Path.Combine(LogDirectory, LoggingFileFormat),
                formatProvider: CultureInfo.InvariantCulture,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: RetainedLoggingFileCountLimit)
            .CreateLogger();
    }
}
