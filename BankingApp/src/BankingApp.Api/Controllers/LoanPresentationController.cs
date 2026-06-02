namespace BankingApp.Api.Controllers;

using Application.Features.Loans;
using Contracts.Http;
using Domain.Aggregates.LoanAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route(ApiEndpoints.LoanPresentation.Base)]
public class LoanPresentationController : ApiControllerBase
{
    [HttpPost]
    public IActionResult GetRepaymentProgress([FromBody] Loan loan)
    {
        double progress = (double)AmortizationCalculator.ComputeRepaymentProgress(
            loan.Principal,
            loan.OutstandingBalance);
        return Ok(progress);
    }
}
