namespace BankingApp.Api.Controllers;

using Application.Features.Billers.Services;
using Contracts.Features.Billers.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route(ApiEndpoints.Billers.Base)]
[Authorize]
public class BillersController(IBillerService billerService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBillers(
        [FromQuery] string? search,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        return ToActionResult(await billerService.GetBillersAsync(search, category, cancellationToken), Ok);
    }

    [HttpGet(ApiEndpoints.Billers.Saved)]
    public async Task<IActionResult> GetSavedBillers(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await billerService.GetSavedBillersAsync(userId, cancellationToken), Ok);
    }

    [HttpPost(ApiEndpoints.Billers.Saved)]
    public async Task<IActionResult> SaveBiller([FromBody] SaveBillerRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await billerService.SaveBillerAsync(userId, request.BillerId, request.Nickname, request.DefaultReference, cancellationToken),
            data => CreatedAtAction(nameof(GetSavedBillers), data));
    }

    [HttpDelete(ApiEndpoints.Billers.SavedById)]
    public async Task<IActionResult> RemoveSavedBiller(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await billerService.DeleteSavedBillerAsync(userId, id, cancellationToken));
    }
}
