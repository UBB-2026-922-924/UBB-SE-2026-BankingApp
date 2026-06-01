namespace BankingApp.Api.Controllers;

using Domain.Aggregates.LoanAggregate;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Loans;

[ApiController]
    [Route(Contracts.Http.ApiEndpoints.LoanPresentation.Base)]
public class LoanPresentationController : ControllerBase
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
