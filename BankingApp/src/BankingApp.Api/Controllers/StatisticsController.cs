namespace BankingApp.Api.Controllers;

using Application.Features.Statistics.Services;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route(ApiEndpoints.Statistics.Base)]
public class StatisticsController(IStatisticsService statisticsService) : ApiControllerBase
{
    [HttpGet(ApiEndpoints.Statistics.SpendingByCategory)]
    public async Task<IActionResult> GetSpendingByCategory(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await statisticsService.GetSpendingByCategoryAsync(userId, cancellationToken), value => Ok(value));
    }

    [HttpGet(ApiEndpoints.Statistics.IncomeVsExpenses)]
    public async Task<IActionResult> GetIncomeVsExpenses(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await statisticsService.GetIncomeVsExpensesAsync(userId, cancellationToken), value => Ok(value));
    }

    [HttpGet(ApiEndpoints.Statistics.BalanceTrends)]
    public async Task<IActionResult> GetBalanceTrends(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await statisticsService.GetBalanceTrendsAsync(userId, cancellationToken), value => Ok(value));
    }

    [HttpGet(ApiEndpoints.Statistics.TopRecipients)]
    public async Task<IActionResult> GetTopRecipients(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await statisticsService.GetTopRecipientsAsync(userId, cancellationToken), value => Ok(value));
    }
}
