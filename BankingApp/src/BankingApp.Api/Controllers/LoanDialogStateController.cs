namespace BankingApp.Api.Controllers;

using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route(ApiEndpoints.LoanDialogState.Base)]
public class LoanDialogStateController : ApiControllerBase
{
    private const int PositiveThreshold = 0;

    [HttpGet]
    public IActionResult GetShouldComputeEstimate(
        [FromQuery] double desiredAmount,
        [FromQuery] int preferredTermMonths,
        [FromQuery] string purpose)
    {
        bool result = desiredAmount > PositiveThreshold &&
                      preferredTermMonths > PositiveThreshold &&
                      !string.IsNullOrWhiteSpace(purpose);
        return Ok(result);
    }
}
