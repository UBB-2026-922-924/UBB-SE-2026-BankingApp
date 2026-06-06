namespace BankingApp.Api.Controllers;

using Application.Features.Savings.Services;
using BankingApp.Contracts.Features.Investments;
using Contracts.Features.Savings.Dtos;
using Contracts.Http;
using Domain.Aggregates.SavingsAggregate;
using Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route(ApiEndpoints.Savings.Base)]
public class SavingsController(ISavingsService savingsService) : ApiControllerBase
{
    [HttpPost(ApiEndpoints.Savings.Accounts)]
    public async Task<IActionResult> CreateAccountAsync(
        [FromBody] CreateSavingsAccountDto account,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(account.SavingsType, ignoreCase: true, out SavingsType savingsType))
        {
            return ToActionResult<SavingsAccount>(
                Error.Validation("Savings.InvalidType", "The selected savings type is invalid."),
                value => Ok(value));
        }

        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await savingsService.CreateAccountAsync(
                userId,
                savingsType,
                account.AccountName,
                account.FundingAccountId,
                account.InitialDeposit,
                account.TargetAmount,
                account.TargetDate,
                account.MaturityDate,
                account.DepositFrequency,
                cancellationToken),
            value => Ok(value));
    }

    [HttpGet(ApiEndpoints.Savings.Accounts)]
    public async Task<IActionResult> GetAccountsAsync(
        [FromQuery] bool includesClosed = false,
        CancellationToken cancellationToken = default)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await savingsService.GetAccountsAsync(userId, includesClosed, cancellationToken), value => Ok(value));
    }

    [HttpPost(ApiEndpoints.Savings.Deposit)]
    public async Task<IActionResult> DepositAsync(
        int accountId,
        [FromQuery] decimal amount,
        [FromQuery] string source,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await savingsService.DepositAsync(userId, accountId, amount, source, cancellationToken),
            value => Ok(new DepositResponseDto
            {
                NewBalance = value.NewBalance,
                TransactionId = value.TransactionId,
                Timestamp = value.Timestamp,
            }));
    }

    [HttpPost(ApiEndpoints.Savings.Withdraw)]
    public async Task<IActionResult> WithdrawAsync(
        int accountId,
        [FromQuery] decimal amount,
        [FromQuery] string destinationLabel,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await savingsService.WithdrawAsync(userId, accountId, amount, destinationLabel, cancellationToken),
            value => Ok(new WithdrawResponseDto
            {
                Success = true,
                AmountWithdrawn = value.AmountWithdrawn,
                PenaltyApplied = value.PenaltyApplied,
                NewBalance = value.NewBalance,
                Message = "Withdrawal processed successfully.",
                ProcessedAt = DateTime.UtcNow,
            }));
    }

    [HttpPost(ApiEndpoints.Savings.Close)]
    public async Task<IActionResult> CloseAccountAsync(
        int accountId,
        [FromQuery] int destinationAccountId,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await savingsService.CloseAccountAsync(userId, accountId, destinationAccountId, cancellationToken),
            value => Ok(new ClosureResultDto
            {
                Success = true,
                TransferredAmount = value.TransferredAmount,
                PenaltyApplied = value.PenaltyApplied,
                Message = "Account closed successfully.",
                ClosedAt = DateTime.UtcNow,
            }));
    }

    [HttpGet(ApiEndpoints.Savings.AutoDepositByAccount)]
    public async Task<IActionResult> GetAutoDepositAsync(int accountId, CancellationToken cancellationToken)
    {
        return ToActionResult(await savingsService.GetAutoDepositAsync(accountId, cancellationToken), value => Ok(value));
    }

    [HttpPost(ApiEndpoints.Savings.AutoDeposit)]
    public async Task<IActionResult> SaveAutoDepositAsync(
        [FromBody] AutoDepositUpsertDto autoDeposit,
        CancellationToken cancellationToken)
    {
        return ToActionResult(await savingsService.SaveAutoDepositAsync(autoDeposit.ToAutoDeposit(), cancellationToken));
    }

    [HttpGet(ApiEndpoints.Savings.FundingSources)]
    [HttpGet(ApiEndpoints.Savings.FundingSources)]
    public async Task<IActionResult> GetFundingSourcesAsync(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await savingsService.GetFundingSourcesAsync(userId, cancellationToken),
            value => Ok(value.Select(source => new FundingSourceOption
            {
                Id = source.Id,
                DisplayName = source.DisplayName
            }).ToList()));
    }

    [HttpGet(ApiEndpoints.Savings.Transactions)]
    public async Task<IActionResult> GetTransactionsAsync(
        int accountId,
        [FromQuery] string filter = "",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return ToActionResult(
            await savingsService.GetTransactionsAsync(accountId, filter, page, pageSize, cancellationToken),
            value => Ok(new GetTransactionsResponse
            {
                Items = value.Items.ToList(),
                TotalCount = value.TotalCount,
                Page = page,
                PageSize = pageSize,
            }));
    }

    [HttpGet(ApiEndpoints.Savings.ValidDestinations)]
    public async Task<IActionResult> GetValidTransferDestinationsAsync(
        int currentAccountId,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await savingsService.GetAccountsAsync(userId, includesClosed: false, cancellationToken),
            accounts => Ok(accounts.Where(account => account.Id != currentAccountId).ToList()));
    }
}
