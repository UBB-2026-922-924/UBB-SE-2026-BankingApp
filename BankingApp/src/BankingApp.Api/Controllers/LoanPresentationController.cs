namespace BankingApp.Api.Controllers;

using BankingApp.Domain.Aggregates.LoanAggregate;
using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Features.Loans;

[ApiController]
[Route("api/loans/repayment-progress")]
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
