namespace BankingApp.Api.Controllers;

using Application.Features.Loans.Services;
using Contracts.Features.Loans.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route(ApiEndpoints.Loans.Base)]
public class LoansController(ILoansService loansService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLoansByUserAsync(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await loansService.GetLoansByUserAsync(userId, cancellationToken), value => Ok(value));
    }

    [HttpGet(ApiEndpoints.Loans.ById)]
    public async Task<IActionResult> GetLoanByIdAsync(int id, CancellationToken cancellationToken)
    {
        return ToActionResult(await loansService.GetLoanByIdAsync(id, cancellationToken), value => Ok(value));
    }

    [HttpPost(ApiEndpoints.Loans.Applications)]
    public async Task<IActionResult> SubmitApplicationAsync(
        [FromBody] LoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        request.UserId = userId;
        return ToActionResult(await loansService.SubmitApplicationAsync(request, cancellationToken), value => Ok(value));
    }

    [HttpPost(ApiEndpoints.Loans.Estimate)]
    public IActionResult GetEstimate([FromBody] LoanApplicationRequest request)
    {
        int userId = GetAuthenticatedUserId();
        request.UserId = userId;
        return ToActionResult(loansService.GetEstimate(request), value => Ok(value));
    }

    [HttpPut(ApiEndpoints.Loans.PayInstallment)]
    public async Task<IActionResult> PayInstallmentAsync(
        int loanId,
        [FromQuery] decimal? customAmount,
        CancellationToken cancellationToken)
    {
        return ToActionResult(await loansService.PayInstallmentAsync(loanId, customAmount, cancellationToken));
    }

    [HttpGet(ApiEndpoints.Loans.AmortizationSchedule)]
    public async Task<IActionResult> GetAmortizationAsync(int loanId, CancellationToken cancellationToken)
    {
        return ToActionResult(await loansService.GetAmortizationAsync(loanId, cancellationToken), value => Ok(value));
    }
}
