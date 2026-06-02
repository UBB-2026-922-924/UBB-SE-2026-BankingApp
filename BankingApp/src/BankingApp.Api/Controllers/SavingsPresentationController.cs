namespace BankingApp.Api.Controllers;

using Contracts.Features.Savings.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route(ApiEndpoints.SavingsPresentation.Base)]
public class SavingsPresentationController : ApiControllerBase
{
    private const int SingularAccountCount = 1;
    private const decimal DefaultBestApy = 0m;
    private const decimal PercentageScale = 100m;

    [HttpPost(ApiEndpoints.SavingsPresentation.TotalSaved)]
    public ActionResult<string> GetTotalSavedAmount([FromBody] IEnumerable<SavingsAccountSnapshotDto> accounts)
    {
        string result = $"${accounts.Sum(account => account.Balance):F2}";
        return Ok(result);
    }

    [HttpGet($"{ApiEndpoints.SavingsPresentation.AccountsText}/{{accountCount}}")]
    public ActionResult<string> GetNumberOfAccountsText([FromRoute] int accountCount)
    {
        string result = $"across {accountCount} account{(accountCount == SingularAccountCount ? string.Empty : "s")}";
        return Ok(result);
    }

    [HttpPost(ApiEndpoints.SavingsPresentation.BestInterestRate)]
    public ActionResult<string> GetBestInterestRate([FromBody] IEnumerable<SavingsAccountSnapshotDto> accounts)
    {
        decimal bestApy = accounts.Any() ? accounts.Max(account => account.AnnualPercentageYield) : DefaultBestApy;
        string result = $"{bestApy * PercentageScale:F2}%";
        return Ok(result);
    }

    [HttpPost(ApiEndpoints.SavingsPresentation.ClosePenaltyRisk)]
    public ActionResult<bool> CheckClosePenaltyRisk([FromBody] SavingsAccountSnapshotDto selectedAccount)
    {
        bool hasRisk = selectedAccount?.SavingsType == "FixedDeposit" &&
                       selectedAccount.MaturityDate.HasValue &&
                       selectedAccount.MaturityDate.Value > DateTime.UtcNow;
        return Ok(hasRisk);
    }
}
