namespace BankingApp.Infrastructure.Core.DependencyInjection;

using Application;
using Application.Features.Forex.Services;
using Application.Shared.Clock;
using Caching;
using Clock;
using ExchangeRates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<ILockedRateCache, MemoryLockedRateCache>();
        services.AddSingleton<IExchangeRateService, ConfigurationExchangeRateService>();

        return services;
    }
}
