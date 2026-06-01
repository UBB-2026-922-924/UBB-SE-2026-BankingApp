namespace BankingApp.Api.Controllers;

using Contracts.Features.Loans.Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
    [Route(Contracts.Http.ApiEndpoints.LoanApplicationPresentation.Base)]
public class LoanApplicationPresentationController : ControllerBase
{
    [HttpGet]
    public IActionResult GetBuildApplicationOutcome([FromQuery] string? rejectionReason)
    {
        var result = new BuildApplicationOutcomeResponse
        {
            IsApproved = string.IsNullOrWhiteSpace(rejectionReason),
            Message = string.IsNullOrWhiteSpace(rejectionReason)
                ? "Your loan application has been approved!"
                : $"Application rejected: {rejectionReason}",
        };

        return Ok(result);
    }
}
