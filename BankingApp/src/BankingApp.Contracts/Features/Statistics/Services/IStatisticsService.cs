namespace BankingApp.Contracts.Features.Statistics.Services;

using BankingApp.Contracts.Features.Statistics.Dtos;
using ErrorOr;

public interface IStatisticsService
{
    public Task<ErrorOr<SpendingByCategoryResponse>> GetSpendingByCategoryAsync(CancellationToken ct = default);
    public Task<ErrorOr<IncomeVsExpensesResponse>> GetIncomeVsExpensesAsync(CancellationToken ct = default);
    public Task<ErrorOr<BalanceTrendsResponse>> GetBalanceTrendsAsync(CancellationToken ct = default);
    public Task<ErrorOr<TopRecipientsResponse>> GetTopRecipientsAsync(CancellationToken ct = default);

}
