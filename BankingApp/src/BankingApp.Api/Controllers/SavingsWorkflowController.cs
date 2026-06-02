namespace BankingApp.Api.Controllers;

using Contracts.Features.Investments;
using Contracts.Features.Savings.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route(ApiEndpoints.SavingsWorkflow.Base)]
public class SavingsWorkflowController : ApiControllerBase
{
    private const int NoDestinationId = 0;
    private const decimal PositiveAmountThreshold = 0m;
    private const decimal NoPenaltyAmount = 0m;
    private const int FirstPage = 1;

    [HttpPost(ApiEndpoints.SavingsWorkflow.DefaultFundingSource)]
    public ActionResult<FundingSourceOption> GetDefaultFundingSource([FromBody] IEnumerable<FundingSourceOption>? fundingSources)
    {
        if (fundingSources == null)
        {
            return BadRequest("List of funding sources cannot be null.");
        }

        FundingSourceOption? result = fundingSources.FirstOrDefault();
        if (result == null)
        {
            return NoContent();
        }

        return Ok(result);
    }

    [HttpPost(ApiEndpoints.SavingsWorkflow.DefaultCloseDestination)]
    public ActionResult<int> GetDefaultCloseDestinationId([FromBody] IEnumerable<SavingsAccountSnapshotDto>? destinationAccounts)
    {
        if (destinationAccounts == null)
        {
            return BadRequest("List of accounts cannot be null.");
        }

        int destinationId = destinationAccounts.FirstOrDefault()?.IdentificationNumber ?? NoDestinationId;
        return Ok(destinationId);
    }

    [HttpPost(ApiEndpoints.SavingsWorkflow.ValidateWithdraw)]
    public ActionResult ValidateWithdrawRequest([FromQuery] decimal amount, [FromBody] FundingSourceOption? destination)
    {
        (bool IsValid, string ErrorMessage) result;

        if (amount <= PositiveAmountThreshold)
        {
            result = (false, "Please enter a valid amount.");
        }
        else if (destination == null)
        {
            result = (false, "Please select a destination account.");
        }
        else
        {
            result = (true, string.Empty);
        }

        return Ok(new { result.IsValid, result.ErrorMessage });
    }

    [HttpPost(ApiEndpoints.SavingsWorkflow.WithdrawResultMessage)]
    public ActionResult<string> BuildWithdrawResultMessage([FromBody] WithdrawResponseDto response)
    {
        string message;
        if (!response.Success)
        {
            message = response.Message;
        }
        else
        {
            string penaltyText = response.PenaltyApplied > NoPenaltyAmount
                ? $" (penalty: ${response.PenaltyApplied:N2})"
                : string.Empty;
            message = $"Withdrawn: ${response.AmountWithdrawn:N2}{penaltyText}. New balance: ${response.NewBalance:N2}";
        }

        return Ok(message);
    }

    [HttpGet(ApiEndpoints.SavingsWorkflow.ValidateClose)]
    public ActionResult ValidateCloseConfirmation([FromQuery] bool userConfirmed, [FromQuery] int destinationId)
    {
        (bool IsValid, string ErrorMessage) result;

        if (!userConfirmed)
        {
            result = (false, "Please confirm account closure.");
        }
        else if (destinationId == NoDestinationId)
        {
            result = (false, "Please select a destination account.");
        }
        else
        {
            result = (true, string.Empty);
        }

        return Ok(new { result.IsValid, result.ErrorMessage });
    }

    [HttpGet(ApiEndpoints.SavingsWorkflow.CanMoveNext)]
    public ActionResult<bool> CanMoveToNextPage([FromQuery] int currentPage, [FromQuery] int totalPages)
    {
        bool canMove = currentPage < totalPages;
        return Ok(canMove);
    }

    [HttpGet(ApiEndpoints.SavingsWorkflow.CanMovePrevious)]
    public ActionResult<bool> CanMoveToPreviousPage([FromQuery] int currentPage)
    {
        bool canMove = currentPage > FirstPage;
        return Ok(canMove);
    }
}
