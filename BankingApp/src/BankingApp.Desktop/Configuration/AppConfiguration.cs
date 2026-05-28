namespace BankingApp.Desktop.Configuration;

using System;
using Microsoft.Extensions.Configuration;

internal static class AppConfiguration
{
    public static IConfigurationRoot Build()
    {
        string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        bool isDevelopment = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);

        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true);

        if (isDevelopment)
        {
            builder.AddUserSecrets(typeof(AppConfiguration).Assembly, optional: true);
        }

        return builder
            .AddEnvironmentVariables()
            .Build();
    }
}
