namespace BankingApp.Application.Features.AccountOverview.Services;

using Contracts.Features.AccountOverview.Dtos;
using ErrorOr;

public interface IAccountOverviewService
{
    public Task<ErrorOr<AccountOverviewDto>> GetDashboardAsync(int userId, CancellationToken cancellationToken = default);
}
