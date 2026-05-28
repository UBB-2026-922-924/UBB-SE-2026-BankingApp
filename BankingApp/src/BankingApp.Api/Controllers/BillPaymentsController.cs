namespace BankingApp.Api.Controllers;

using Application.Features.BillPayments.Services;
using Contracts.Features.BillPayments.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Handles operations related to bill payments.
/// </summary>
[ApiController]
[Route(ApiEndpoints.BillPayments.Base)]
[Authorize]
public class BillPaymentsController(IBillPaymentService billPaymentService) : ApiControllerBase
{
    private const decimal LowTierFee = 0.50m;
    private const decimal HighTierFee = 1.00m;
    private const decimal FeeThreshold = 100m;

    /// <summary>
    /// Returns the list of active accounts for the current user to fund a bill payment.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The user's active accounts.</returns>
    [HttpGet(ApiEndpoints.BillPayments.Accounts)]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await billPaymentService.GetAccountsAsync(userId, cancellationToken), Ok);
    }

    /// <summary>
    /// Calculates the fee for a bill payment based on the amount.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <returns>A response containing the calculated fee.</returns>
    [HttpGet(ApiEndpoints.BillPayments.Fee)]
    public IActionResult CalculateFee([FromQuery] decimal amount)
    {
        decimal fee = amount <= FeeThreshold ? LowTierFee : HighTierFee;
        return Ok(new FeeResponse { Fee = fee });
    }

    /// <summary>
    /// Processes a bill payment request.
    /// </summary>
    /// <param name="request">The bill payment request details.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The result of the payment processing.</returns>
    [HttpPost(ApiEndpoints.BillPayments.Pay)]
    public async Task<IActionResult> ProcessPayment([FromBody] BillPayRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await billPaymentService.ProcessAsync(
                userId, request.SourceAccountId, request.BillerId,
                request.BillerReference, request.Amount, cancellationToken),
            Ok);
    }

    /// <summary>
    /// Retrieves the bill payment history for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The bill payment history.</returns>
    [HttpGet(ApiEndpoints.BillPayments.History)]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await billPaymentService.GetHistoryAsync(userId, cancellationToken), Ok);
    }
}
