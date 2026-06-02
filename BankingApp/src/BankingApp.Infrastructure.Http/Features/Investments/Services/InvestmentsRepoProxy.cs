namespace BankingApp.Infrastructure.Http.Features.Investments.Services;

using Shared.Http;

public class InvestmentsRepoProxy(ApiService api) : IInvestmentsRepoProxy
{
    public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
    {
        return await api.GetAsync<TResponse>(endpoint);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        return await api.PostAsync<TRequest, TResponse>(endpoint, data);
    }
}
