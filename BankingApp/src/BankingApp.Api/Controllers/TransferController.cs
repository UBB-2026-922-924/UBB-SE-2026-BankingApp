namespace BankingApp.Api.Controllers;

using Application.Features.Transfers.Services;
using Contracts.Features.Transfers.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route(ApiEndpoints.Transfers.Base)]
[Route(ApiEndpoints.Transfers.LegacyBase)]
public class TransferController(ITransferService transferService) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await transferService.ExecuteAsync(
                userId, request.SourceAccountId, request.RecipientName,
                request.RecipientIban, request.Amount, request.Currency, request.Reference, cancellationToken),
            transfer => CreatedAtAction(nameof(GetHistory), new { }, transfer));
    }

    [HttpPost(ApiEndpoints.Transfers.Execute)]
    public async Task<IActionResult> ExecuteTransfer([FromBody] CreateTransferRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await transferService.ExecuteAsync(
                userId, request.SourceAccountId, request.RecipientName,
                request.RecipientIban, request.Amount, request.Currency, request.Reference, cancellationToken),
            transfer => Ok(new TransferExecutionResponse { TransactionRef = transfer.TransactionRef ?? string.Empty }));
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await transferService.GetHistoryAsync(userId, cancellationToken), Ok);
    }

    [HttpGet(ApiEndpoints.Transfers.Accounts)]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await transferService.GetAccountsAsync(userId, cancellationToken), Ok);
    }

    [HttpPost(ApiEndpoints.Transfers.ValidateIban)]
    public async Task<IActionResult> ValidateIban([FromBody] TransferIbanValidationRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(await transferService.ValidateIbanAsync(request.Iban, cancellationToken), Ok);
    }

    [HttpGet(ApiEndpoints.Transfers.FxPreview)]
    public async Task<IActionResult> GetFxPreview([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount, CancellationToken cancellationToken)
    {
        return ToActionResult(await transferService.GetFxPreviewAsync(from, to, amount, cancellationToken), Ok);
    }
}
