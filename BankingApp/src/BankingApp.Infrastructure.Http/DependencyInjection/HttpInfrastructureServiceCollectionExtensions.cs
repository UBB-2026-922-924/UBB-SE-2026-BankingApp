namespace BankingApp.Infrastructure.Http.DependencyInjection;

using Application.Features.Authentication.Services;
using Application.Shared.Http;
using Contracts.Features.AccountOverview.Services;
using Contracts.Features.Beneficiaries.Services;
using Contracts.Features.Billers.Services;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.Cards.Services;
using Contracts.Features.Forex.Services;
using Contracts.Features.Transfers.Services;
using Contracts.Features.UserProfile.Services;
using Features.AccountOverview.Services;
using Features.Authentication.Services;
using Features.Beneficiaries.Services;
using Features.Billers.Services;
using Features.BillPayments.Services;
using Features.Cards.Services;
using Features.Chat.Services;
using Features.Forex.Services;
using Features.Investments.Services;
using Features.Loans.Services;
using Features.Savings.Services;
using Features.Statistics.Services;
using Features.Transfers.Services;
using Features.UserProfile.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Http;

public static class HttpInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddHttpInfrastructure(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.TryAdd(ServiceDescriptor.Describe(typeof(IApiClient), typeof(ApiClient), lifetime));
        services.TryAdd(ServiceDescriptor.Describe(typeof(ApiService), typeof(ApiService), lifetime));

        services.Add(ServiceDescriptor.Describe(typeof(IAuthenticationService), typeof(AuthenticationService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IAccountOverviewService), typeof(AccountOverview), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IBeneficiaryService), typeof(BeneficiaryService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IBillPaymentService), typeof(BillPaymentService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IBillerService), typeof(BillerService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ICardService), typeof(CardService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IForexService), typeof(ForexService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ITransferService), typeof(TransferService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IProfileService), typeof(ProfileService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IInvestmentsRepoProxy), typeof(InvestmentsRepoProxy), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ISavingsRepoProxy), typeof(SavingsRepoProxy), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ISavingsWorkflowRepoProxy), typeof(SavingsWorkflowRepoProxy), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ISavingsUiRulesRepoProxy), typeof(SavingsUiRulesRepoProxy), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ISavingsPresentationRepoProxy), typeof(SavingsPresentationRepoProxy), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ILoansRepoProxy), typeof(LoansRepoProxy), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ILoanDialogStateRepoProxy), typeof(LoanDialogStateRepoProxy), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ILoanApplicationPresentationRepoProxy), typeof(LoanApplicationPresentationRepoProxy), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IStatisticsRepoProxy), typeof(StatisticsRepoProxy), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IChatRepoProxy), typeof(ChatRepoProxy), lifetime));

        return services;
    }
}
