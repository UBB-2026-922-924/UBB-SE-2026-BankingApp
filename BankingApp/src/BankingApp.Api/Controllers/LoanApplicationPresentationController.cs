using BankingApp.Contracts.Features.Loans.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers
{
    [ApiController]
    [Route("api/loans/loan-application-presentation-outcome")]
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
}
