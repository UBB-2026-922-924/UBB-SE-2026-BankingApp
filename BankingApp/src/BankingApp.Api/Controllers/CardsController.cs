namespace BankingApp.Api.Controllers;

using Application.Features.Cards.Services;
using Contracts.Features.Cards.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route(ApiEndpoints.Cards.Base)]
public class CardsController(ICardService cardService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await cardService.GetCardsAsync(userId, cancellationToken), Ok);
    }

    [HttpPut(ApiEndpoints.Cards.Freeze)]
    public async Task<IActionResult> Freeze(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await cardService.FreezeAsync(userId, id, cancellationToken));
    }

    [HttpPut(ApiEndpoints.Cards.Unfreeze)]
    public async Task<IActionResult> Unfreeze(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await cardService.UnfreezeAsync(userId, id, cancellationToken));
    }

    [HttpDelete(ApiEndpoints.Cards.ById)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await cardService.CancelAsync(userId, id, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Issue([FromBody] IssueCardRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await cardService.IssueAsync(userId, request.CardType, request.CardBrand, cancellationToken), Ok);
    }
}
