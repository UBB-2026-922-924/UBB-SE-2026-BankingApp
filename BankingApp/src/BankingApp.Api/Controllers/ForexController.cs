namespace BankingApp.Api.Controllers;

using Application.Features.Forex.Services;
using Contracts.Features.Forex.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route(ApiEndpoints.Forex.Base)]
public class ForexController(IForexService forexService) : ApiControllerBase
{
    [HttpGet(ApiEndpoints.Forex.Preview)]
    public async Task<IActionResult> GetPreview(
        [FromQuery] string sourceCurrency,
        [FromQuery] string targetCurrency,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await forexService.GetRatePreviewAsync(userId, sourceCurrency, targetCurrency, amount, cancellationToken),
            Ok);
    }

    [HttpPost(ApiEndpoints.Forex.Execute)]
    public async Task<IActionResult> Execute([FromBody] ForexTransactionRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await forexService.ExecuteAsync(
                userId, request.SourceAccountId, request.TargetAccountId,
                request.SourceCurrency, request.TargetCurrency, request.SourceAmount, cancellationToken),
            Ok);
    }

    [HttpGet(ApiEndpoints.Forex.History)]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await forexService.GetHistoryAsync(userId, cancellationToken), Ok);
    }
}
