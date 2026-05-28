namespace BankingApp.Infrastructure.Http.Features.AccountOverview.Services;

using Application.Shared.Http;
using Contracts.Features.AccountOverview.Dtos;
using Contracts.Features.AccountOverview.Services;
using Contracts.Http;
using ErrorOr;

public sealed class AccountOverview(IApiClient apiClient) : IAccountOverviewService
{
    public Task<ErrorOr<AccountOverviewDto>> GetDashboardAsync(CancellationToken ct = default)
        => apiClient.GetAsync<AccountOverviewDto>(ApiEndpoints.AccountOverview.Base, ct);
}
