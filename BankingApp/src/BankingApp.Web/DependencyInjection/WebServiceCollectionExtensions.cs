namespace BankingApp.Web.DependencyInjection;

using BankingApp.Contracts.Http;
using BankingApp.Infrastructure.Http.DependencyInjection;
using Http;
using Microsoft.Extensions.DependencyInjection;

public static class WebServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWebClientServices(string apiBaseUrl)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<BearerTokenForwardingHandler>();

            services.AddHttpClient(HttpClientNames.Api, client =>
                    client.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<BearerTokenForwardingHandler>();

            services.AddHttpInfrastructure(ServiceLifetime.Scoped);

            return services;
        }
    }
}
