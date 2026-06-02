using BankingApp.Contracts.Features.Savings.Dtos;
using BankingApp.Domain.Aggregates.SavingsAggregate;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers
{
    [ApiController]
    [Route("api/savings-presentation")]
    public class SavingsPresentationController : ControllerBase
    {
        private const int SingularAccountCount = 1;
        private const decimal DefaultBestApy = 0m;
        private const decimal PercentageScale = 100m;

        [HttpPost("total-saved")]
        public ActionResult<string> GetTotalSavedAmount([FromBody] IEnumerable<SavingsAccountSnapshotDto> accounts)
        {
            string result = $"${accounts.Sum(account => account.Balance):F2}";
            return Ok(result);
        }

        [HttpGet("accounts-text/{accountCount}")]
        public ActionResult<string> GetNumberOfAccountsText([FromRoute] int accountCount)
        {
            string result = $"across {accountCount} account{(accountCount == SingularAccountCount ? string.Empty : "s")}";
            return Ok(result);
        }

        [HttpPost("best-interest-rate")]
        public ActionResult<string> GetBestInterestRate([FromBody] IEnumerable<SavingsAccountSnapshotDto> accounts)
        {
            decimal bestApy = accounts.Any() ? accounts.Max(account => account.AnnualPercentageYield) : DefaultBestApy;
            string result = $"{bestApy * PercentageScale:F2}%";
            return Ok(result);
        }

        [HttpPost("close-penalty-risk")]
        public ActionResult<bool> CheckClosePenaltyRisk([FromBody] SavingsAccountSnapshotDto selectedAccount)
        {
            bool hasRisk = selectedAccount?.SavingsType == "FixedDeposit" &&
                           selectedAccount.MaturityDate.HasValue &&
                           selectedAccount.MaturityDate.Value > DateTime.UtcNow;
            return Ok(hasRisk);
        }
    }
}
