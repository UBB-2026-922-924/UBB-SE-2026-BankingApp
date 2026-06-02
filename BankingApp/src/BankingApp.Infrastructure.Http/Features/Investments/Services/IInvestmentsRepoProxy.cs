namespace BankingApp.Infrastructure.Http.Features.Investments.Services
{
    using System.Threading.Tasks;
    using BankingApp.Domain.Aggregates.InvestmentAggregate;

    public interface IInvestmentsRepoProxy
    {
        Task<TResponse?> GetAsync<TResponse>(string endpoint);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
    }
}