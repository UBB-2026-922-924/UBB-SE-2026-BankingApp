namespace BankingApp.Api.Controllers;

using Application.Features.Transactions.Services;
using Contracts.Features.Transactions.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>Exposes transaction history, detail, and export endpoints for the authenticated user.</summary>
[ApiController]
[Authorize]
[Route(ApiEndpoints.Transactions.Base)]
public class TransactionsController(ITransactionService transactionService) : ApiControllerBase
{
    [HttpGet(ApiEndpoints.Transactions.Filters)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await transactionService.GetFilterMetadataAsync(userId, cancellationToken), Ok);
    }

    [HttpPost(ApiEndpoints.Transactions.History)]
    public async Task<IActionResult> GetHistory(
        [FromBody] TransactionHistoryRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await transactionService.GetHistoryAsync(userId, request, cancellationToken), Ok);
    }

    [HttpGet(ApiEndpoints.Transactions.ById)]
    public async Task<IActionResult> GetTransaction(int transactionId, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await transactionService.GetTransactionByIdAsync(userId, transactionId, cancellationToken), Ok);
    }

    [HttpPost(ApiEndpoints.Transactions.Export)]
    public async Task<IActionResult> ExportTransactions(
        [FromBody] TransactionExportRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await transactionService.ExportTransactionsAsync(userId, request, cancellationToken),
            result => File(result.Content, result.ContentType, result.FileName));
    }

    [HttpGet(ApiEndpoints.Transactions.Receipt)]
    public async Task<IActionResult> ExportReceipt(int transactionId, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await transactionService.ExportReceiptAsync(userId, transactionId, cancellationToken),
            result => File(result.Content, result.ContentType, result.FileName));
    }
}