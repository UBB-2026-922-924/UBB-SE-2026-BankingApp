namespace BankingApp.Infrastructure.Http.Features.Statistics.Services;

using BankingApp.Contracts.Features.Statistics.Dtos;

public interface IStatisticsRepoProxy
{
    public Task<SpendingByCategoryResponse> GetSpendingByCategoryAsync();

    public Task<IncomeVsExpensesResponse> GetIncomeVsExpensesAsync();

    public Task<BalanceTrendsResponse> GetBalanceTrendsAsync();

    public Task<TopRecipientsResponse> GetTopRecipientsAsync();
}
