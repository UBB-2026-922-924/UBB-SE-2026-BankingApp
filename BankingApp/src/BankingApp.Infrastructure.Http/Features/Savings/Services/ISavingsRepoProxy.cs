namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using BankingApp.Contracts.Features.Savings.Dtos;
using Domain.Aggregates.SavingsAggregate;
using BankingApp.Contracts.Features.Investments;
using Domain.Aggregates.SavingsAggregate.Entities;

/// <summary>
/// RepoProxy for Savings (desktop -> HTTP API).
/// Low-level persistence operations only; business rules live in desktop services.
/// </summary>
public interface ISavingsRepoProxy
{
    public Task<SavingsAccount> CreateSavingsAccountAsync(CreateSavingsAccountDto account, decimal apy);

    public Task<List<SavingsAccount>> GetSavingsAccountsByUserIdAsync(int userId, bool includesClosed = false);

    public Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source);

    public Task<WithdrawResponseDto> WithdrawAsync(int accountId, decimal amount, string destinationLabel, decimal earlyWithdrawalPenalty);

    public Task<ClosureResultDto> CloseSavingsAccountAsync(int accountId, int destinationAccountId, decimal transferAmount, decimal earlyClosurePenalty);

    public Task<AutoDeposit> GetAutoDepositAsync(int accountId);

    public Task SaveAutoDepositAsync(AutoDeposit autoDeposit);

    public Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId);

    public Task<GetTransactionsResponse> GetTransactionsAsync(int accountId, string filter = "", int page = 1, int pageSize = 20);

    public Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId, int userId);

    public Task<decimal> GetPenaltyDecimalFor(string penaltyCase);
}
