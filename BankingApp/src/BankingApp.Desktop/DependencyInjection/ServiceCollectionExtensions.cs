namespace BankingApp.Desktop.DependencyInjection;

using System;
using Application.Shared.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Http;
using Infrastructure.Core.DependencyInjection;
using Infrastructure.Http.DependencyInjection;
using Infrastructure.Http.Shared.Http;
using Navigation;
using Session;
using State;
using Shared.Timers;
using ViewModels;
using Views;

/// <summary>Registers the desktop application's client services, view models, and views.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the Desktop layer dependencies required by the WinUI client.</summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="configuration">Application configuration used by HTTP and client services.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
        services.AddHttpClient(HttpClientNames.Api, client =>
        {
            string? baseUrl = configuration["ApiBaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl);
            }
        });

        services.AddSingleton<IApiClient, ApiClient>();
        services.AddCoreInfrastructure(configuration);
        services.AddHttpInfrastructure();

        services.AddDesktopSession();
        services.AddDesktopState();
        services.AddDesktopServices();
        services.AddDesktopViewModels();
        services.AddDesktopViews();

        return services;
    }

    private static IServiceCollection AddDesktopSession(this IServiceCollection services)
    {
        services.AddSingleton<IAuthenticationSession, AuthenticationSession>();
        services.AddSingleton<ILoginPreferences, FileLoginPreferences>();

        return services;
    }

    private static IServiceCollection AddDesktopState(this IServiceCollection services)
    {
        services.AddSingleton<ILoginNotificationState, LoginNotificationState>();
        services.AddSingleton<ITransferDraftState, TransferDraftState>();

        return services;
    }

    private static IServiceCollection AddDesktopServices(this IServiceCollection services)
    {
        services.AddSingleton<IAppNavigationService, AppNavigationService>();
        services.AddTransient<ICountdownTimer, DispatcherCountdownTimer>();

        return services;
    }

    private static IServiceCollection AddDesktopViewModels(this IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PersonalInfoViewModel>();
        services.AddTransient<SecurityViewModel>();
        services.AddTransient<NotificationsViewModel>();
        services.AddTransient<SessionsViewModel>();
        services.AddTransient<BeneficiariesViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<ForexViewModel>();
        services.AddTransient<TransferViewModel>();
        services.AddTransient<TransferHistoryViewModel>();
        services.AddTransient<BillPayViewModel>();

        services.AddTransient<CardViewModel>();

        return services;
    }

    private static IServiceCollection AddDesktopViews(this IServiceCollection services)
    {
        services.AddTransient<LoginView>();
        services.AddTransient<RegisterView>();
        services.AddTransient<NavigationView>();
        services.AddTransient<DashboardView>();
        services.AddTransient<BeneficiariesView>();
        services.AddTransient<ProfileView>();
        services.AddTransient<ForexPage>();
        services.AddTransient<BillPayView>();
        services.AddTransient<TransferView>();
        services.AddTransient<TransferHistoryView>();
        services.AddTransient<CardsView>();

        return services;
    }
}
