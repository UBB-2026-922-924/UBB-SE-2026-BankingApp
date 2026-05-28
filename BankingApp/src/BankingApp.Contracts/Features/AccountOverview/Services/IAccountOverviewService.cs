namespace BankingApp.Contracts.Features.AccountOverview.Services;

using Dtos;
using ErrorOr;

public interface IAccountOverviewService
{
    public Task<ErrorOr<AccountOverviewDto>> GetDashboardAsync(CancellationToken ct = default);
}
