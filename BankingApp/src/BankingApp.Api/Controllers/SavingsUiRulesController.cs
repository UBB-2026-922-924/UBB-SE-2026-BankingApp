namespace BankingApp.Api.Controllers;

using System.Globalization;
using BankingApp.Contracts.Features.Savings.Dtos;
using BankingApp.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

[ApiController]
    [Route(BankingApp.Contracts.Http.ApiEndpoints.SavingsUiRules.Base)]
public class SavingsUiRulesController : ControllerBase
{
    private const decimal PositiveAmountThreshold = 0m;
    private const int NoPages = 0;

        [HttpGet(BankingApp.Contracts.Http.ApiEndpoints.SavingsUiRules.ParsePositiveAmount)]
    public ActionResult<decimal> ParsePositiveAmount([FromQuery] string text)
    {
        bool isValid = SavingsUiRulesController.TryParsePositiveAmount(text, out decimal amount);
        if (isValid)
        {
            return Ok(amount);
        }
        return BadRequest("Invalid amount. Please enter a positive number.");
    }

        [HttpPost(BankingApp.Contracts.Http.ApiEndpoints.SavingsUiRules.DepositPreview)]
    public ActionResult<string> GetDepositPreview([FromQuery] string depositAmountText, [FromBody] SavingsAccountSnapshotDto selectedAccount)
    {
        string previewText;
        bool isDepositTextPositiveAmount = SavingsUiRulesController.TryParsePositiveAmount(depositAmountText, out decimal amount);

        if (selectedAccount == null || !isDepositTextPositiveAmount)
        {
            previewText = string.Empty;
        }
        else
        {
            previewText = $"New balance will be: ${selectedAccount.Balance + amount:N2}";
        }
        return Ok(previewText);
    }

        [HttpGet(BankingApp.Contracts.Http.ApiEndpoints.SavingsUiRules.WithdrawNetAmount)]
    public ActionResult<decimal> GetWithdrawNetAmount([FromQuery] decimal requestedAmount, [FromQuery] decimal penalty)
    {
        decimal netAmount = requestedAmount - penalty;
        return Ok(netAmount);
    }

        [HttpGet(BankingApp.Contracts.Http.ApiEndpoints.SavingsUiRules.ParseDepositFrequency)]
    public ActionResult<DepositFrequency> ParseDepositFrequency([FromQuery] string frequencyText)
    {
        bool isValid = Enum.TryParse(frequencyText, out DepositFrequency frequency);
        if (isValid)
        {
            return Ok(frequency);
        }
        return BadRequest();
    }

        [HttpGet(BankingApp.Contracts.Http.ApiEndpoints.SavingsUiRules.TotalPages)]
    public ActionResult<int> GetTotalPages([FromQuery] int totalCount, [FromQuery] int pageSize)
    {
        int pages = pageSize <= NoPages
                  ? NoPages
                  : (int)Math.Ceiling((double)totalCount / pageSize);
        return Ok(pages);
    }

        [HttpPost(BankingApp.Contracts.Http.ApiEndpoints.SavingsUiRules.ValidateCreateAccount)]
    public ActionResult<Dictionary<string, string>> ValidateCreateAccount([FromBody] ValidateCreateAccountRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.SelectedSavingsType))
        {
            errors["SavingsType"] = "Please select an account type.";
        }

        if (string.IsNullOrWhiteSpace(request.AccountName))
        {
            errors["AccountName"] = "Account name is required.";
        }

        if (!SavingsUiRulesController.TryParsePositiveAmount(request.InitialDepositText, out _))
        {
            errors["InitialDeposit"] = "Initial deposit must be a positive number.";
        }

        if (!request.HasFundingSource)
        {
            errors["FundingSource"] = "Please select a funding source.";
        }

        if (string.IsNullOrWhiteSpace(request.SelectedFrequency))
        {
            errors["Frequency"] = "Please select a deposit frequency.";
        }

        if (request.IsGoalSavings)
        {
            if (!request.TargetAmount.HasValue || request.TargetAmount.Value <= PositiveAmountThreshold)
            {
                errors["TargetAmount"] = "Target amount is required.";
            }

            if (!request.TargetDate.HasValue)
            {
                errors["TargetDate"] = "Target date is required.";
            }
            else if (request.TargetDate.Value.Date <= DateTime.Today)
            {
                errors["TargetDate"] = "Target date must be in the future.";
            }
        }

        return Ok(errors);
    }

    // Note: this function was salvaged over from the service, because it has been called multiple times
    //  outside of the function which it is "routed" to
    //  I don't know if it's good to have a function that is not routed to by anything inside an API controller,
    //  this is just a helper function to not have to write out the same long conditional multiple times (DRY)
    private static bool TryParsePositiveAmount(string text, out decimal amount)
    {
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out amount) &&
            amount > PositiveAmountThreshold)
        {
            return true;
        }

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out amount) &&
            amount > PositiveAmountThreshold)
        {
            return true;
        }

        amount = PositiveAmountThreshold;
        return false;
    }
}
