using System.Threading.Tasks;
using BankingApp.Client.RepoProxies;
using BankingApp.Contracts.Features.Statistics.Dtos;

namespace BankingApp.Infrastructure.Http.Features.Statistics.Services
{
    public class StatisticsRepoProxy : IStatisticsRepoProxy
    {
        private readonly ApiService _apiService;

        public StatisticsRepoProxy(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync()
        {
            return _apiService.GetAsync<SpendingByCategoryResponse>("/api/statistics/spending-by-category");
        }

        public Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync()
        {
            return _apiService.GetAsync<IncomeVsExpensesResponse>("/api/statistics/income-vs-expenses");
        }

        public Task<BalanceTrendsResponse?> GetBalanceTrendsAsync()
        {
            return _apiService.GetAsync<BalanceTrendsResponse>("/api/statistics/balance-trends");
        }

        public Task<TopRecipientsResponse?> GetTopRecipientsAsync()
        {
            return _apiService.GetAsync<TopRecipientsResponse>("/api/statistics/top-recipients");
        }
    }
}
