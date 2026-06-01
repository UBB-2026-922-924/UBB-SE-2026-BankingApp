namespace BankingApp.Infrastructure.Http.Features.Statistics.Services;

using BankingApp.Contracts.Features.Statistics.Dtos;

public interface IStatisticsRepoProxy
{
    Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync();

    Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync();

    Task<BalanceTrendsResponse?> GetBalanceTrendsAsync();

    Task<TopRecipientsResponse?> GetTopRecipientsAsync();
}
