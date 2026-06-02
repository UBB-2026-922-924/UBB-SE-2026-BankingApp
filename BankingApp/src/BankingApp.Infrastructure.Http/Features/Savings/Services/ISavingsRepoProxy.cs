using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Contracts.Features.Savings.Dtos;
using BankingApp.Domain.Aggregates.InvestmentAggregate;
using BankingApp.Domain.Aggregates.SavingsAggregate;

namespace BankingApp.Infrastructure.Http.Features.Savings.Services
{
    using Contracts.Features.Investments;
    using Domain.Aggregates.SavingsAggregate.Entities;

    /// <summary>
    /// RepoProxy for Savings (desktop -> HTTP API).
    /// Low-level persistence operations only; business rules live in desktop services.
    /// </summary>
    public interface ISavingsRepoProxy
    {
        Task<SavingsAccount> CreateSavingsAccountAsync(CreateSavingsAccountDto account, decimal apy);

        Task<List<SavingsAccount>> GetSavingsAccountsByUserIdAsync(int userId, bool includesClosed = false);

        Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source);

        Task<WithdrawResponseDto> WithdrawAsync(int accountId, decimal amount, string destinationLabel, decimal earlyWithdrawalPenalty);

        Task<ClosureResultDto> CloseSavingsAccountAsync(int accountId, int destinationAccountId, decimal transferAmount, decimal earlyClosurePenalty);

        Task<AutoDeposit> GetAutoDepositAsync(int accountId);

        Task SaveAutoDepositAsync(AutoDeposit autoDeposit);

        Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId);

        Task<GetTransactionsResponse> GetTransactionsAsync(int accountId, string filter = "", int page = 1, int pageSize = 20);

        Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId, int userId);

        Task<decimal> GetPenaltyDecimalFor(string penaltyCase);
    }
}
