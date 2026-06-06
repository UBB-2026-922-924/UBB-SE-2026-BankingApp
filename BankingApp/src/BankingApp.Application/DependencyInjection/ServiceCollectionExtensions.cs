namespace BankingApp.Application.DependencyInjection;

using System.Reflection;
using Features.Chat.Services;
using Features.AccountOverview.Services;
using Features.Authentication.Services;
using Features.Beneficiaries.Services;
using Features.Billers.Services;
using Features.BillPayments.Services;
using Features.Cards.Services;
using Features.Forex.Services;
using Features.Investments.Services;
using Features.Loans.Services;
using Features.Savings.Services;
using Features.Statistics.Services;
using Features.Transfers.Services;
using Features.UserProfile.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Provides extension methods for registering application-layer services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers application service implementations and validators.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountOverviewService, AccountOverviewService>();
        services.AddScoped<IBeneficiaryService, BeneficiaryService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IBillPaymentService, BillPaymentService>();
        services.AddScoped<IBillerService, BillerService>();
        services.AddScoped<IForexService, ForexService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IInvestmentsService, InvestmentsService>();
        services.AddScoped<ISavingsService, SavingsService>();
        services.AddScoped<ILoansService, LoansService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<ITransactionService, TransactionService>();
        return services;
    }
}
