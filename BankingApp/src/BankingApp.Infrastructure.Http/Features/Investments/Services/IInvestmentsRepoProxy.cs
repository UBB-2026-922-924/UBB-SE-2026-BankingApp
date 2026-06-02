namespace BankingApp.Infrastructure.Http.Features.Investments.Services;

using System.Threading.Tasks;

public interface IInvestmentsRepoProxy
{
    public Task<TResponse?> GetAsync<TResponse>(string endpoint);
    public Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
}
