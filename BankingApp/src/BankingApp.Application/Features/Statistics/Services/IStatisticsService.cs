namespace BankingApp.Application.Features.Statistics.Services;

using Contracts.Features.Statistics.Dtos;
using ErrorOr;

public interface IStatisticsService
{
    public Task<ErrorOr<SpendingByCategoryResponse>> GetSpendingByCategoryAsync(
        int userId,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<IncomeVsExpensesResponse>> GetIncomeVsExpensesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<BalanceTrendsResponse>> GetBalanceTrendsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<TopRecipientsResponse>> GetTopRecipientsAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
