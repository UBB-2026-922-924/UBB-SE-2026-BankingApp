namespace BankingApp.Api.Controllers;

using Application.Features.Beneficiaries.Services;
using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Manages the authenticated user's saved beneficiaries.
/// </summary>
[ApiController]
[Authorize]
[Route(ApiEndpoints.Beneficiaries.Base)]
public class BeneficiariesController(IBeneficiaryService beneficiaryService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBeneficiaries(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await beneficiaryService.GetAllAsync(userId, cancellationToken), Ok);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBeneficiary(
        [FromBody] CreateBeneficiaryRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await beneficiaryService.CreateAsync(userId, request.Name, request.Iban, request.BankName, cancellationToken),
            Ok);
    }

    [HttpPut(ApiEndpoints.Beneficiaries.ById)]
    public async Task<IActionResult> UpdateBeneficiary(
        int id,
        [FromBody] UpdateBeneficiaryRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await beneficiaryService.UpdateAsync(userId, id, request.Name, request.Iban, request.BankName, cancellationToken));
    }

    [HttpDelete(ApiEndpoints.Beneficiaries.ById)]
    public async Task<IActionResult> DeleteBeneficiary(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await beneficiaryService.DeleteAsync(userId, id, cancellationToken));
    }
}
