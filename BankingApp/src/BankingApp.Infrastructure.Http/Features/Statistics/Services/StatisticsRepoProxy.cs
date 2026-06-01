namespace BankingApp.Infrastructure.Http.Features.Statistics.Services;

using BankingApp.Contracts.Features.Statistics.Dtos;
using BankingApp.Contracts.Http;
using Shared.Http;

public class StatisticsRepoProxy(ApiService apiService) : IStatisticsRepoProxy
{
    public Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync()
    {
        return apiService.GetAsync<SpendingByCategoryResponse>(ApiEndpoints.Statistics.SpendingByCategoryFull);
    }

    public Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync()
    {
        return apiService.GetAsync<IncomeVsExpensesResponse>(ApiEndpoints.Statistics.IncomeVsExpensesFull);
    }

    public Task<BalanceTrendsResponse?> GetBalanceTrendsAsync()
    {
        return apiService.GetAsync<BalanceTrendsResponse>(ApiEndpoints.Statistics.BalanceTrendsFull);
    }

    public Task<TopRecipientsResponse?> GetTopRecipientsAsync()
    {
        return apiService.GetAsync<TopRecipientsResponse>(ApiEndpoints.Statistics.TopRecipientsFull);
    }
}
