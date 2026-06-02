namespace BankingApp.Api.Controllers;

using Contracts.Features.Loans.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route(ApiEndpoints.LoanApplicationPresentation.Base)]
public class LoanApplicationPresentationController : ApiControllerBase
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
