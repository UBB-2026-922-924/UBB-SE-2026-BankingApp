namespace BankingApp.Api.Controllers;

using Application.Features.AccountOverview.Services;
using Contracts.Http;
using Domain.Aggregates.AccountAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Exposes the authenticated user's dashboard data.
/// </summary>
[ApiController]
[Authorize]
[Route(ApiEndpoints.AccountOverview.Base)]
public class AccountOverviewController(IAccountOverviewService accountOverviewService, IAccountService accountService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await accountOverviewService.GetDashboardAsync(userId, cancellationToken), Ok);
    }

    [HttpGet(ApiEndpoints.AccountOverview.Accounts)]
    public async Task<ActionResult<List<Account>>> GetAccountsForUser(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();

        IReadOnlyCollection<Account> accounts = await accountService.GetAccountsByUserIdAsync(userId, cancellationToken);
        return Ok(accounts);
    }


}
