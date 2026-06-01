namespace BankingApp.Api.Controllers;

using Application.Features.Investments.Services;
using Contracts.Features.Investments.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route(ApiEndpoints.Investments.Base)]
public class InvestmentsController(IInvestmentsService investmentsService) : ApiControllerBase
{
    [HttpGet(ApiEndpoints.Investments.Portfolio)]
    public async Task<IActionResult> GetPortfolio(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await investmentsService.GetPortfolioAsync(userId, cancellationToken), value => Ok(value));
    }

    [HttpPost(ApiEndpoints.Investments.Trade)]
    public async Task<IActionResult> ExecuteTrade(
        [FromBody] ExecuteTradeRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await investmentsService.ExecuteTradeAsync(
                userId,
                request.Ticker,
                request.ActionType,
                request.Quantity,
                request.PricePerUnit,
                request.Fees,
                cancellationToken));
    }

    [HttpGet(ApiEndpoints.Investments.Logs)]
    public async Task<IActionResult> GetLogs(
        DateTime? from,
        DateTime? to,
        string? ticker,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await investmentsService.GetLogsAsync(userId, from, to, ticker, cancellationToken), value => Ok(value));
    }
}
