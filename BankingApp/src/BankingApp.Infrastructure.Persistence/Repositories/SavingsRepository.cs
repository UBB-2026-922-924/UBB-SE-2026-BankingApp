namespace BankingApp.Infrastructure.Persistence.Repositories;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.SavingsAggregate;
using Domain.Aggregates.SavingsAggregate.Entities;
using BankingApp.Domain.Repositories;
using Data;

/// <summary>EF Core-backed savings repository. Full implementation deferred to Step 3.</summary>
public sealed class SavingsRepository : ISavingsRepository
{
    private readonly AppDbContext _dbContext;

    public SavingsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SavingsAccount> CreateSavingsAccountAsync(SavingsAccount account, CancellationToken cancellationToken)
        => throw new NotImplementedException("Pending Step 3 EF Core implementation.");

    public Task<IReadOnlyCollection<SavingsAccount>> GetSavingsAccountsByUserIdAsync(int userId, bool includesClosedAccounts, CancellationToken cancellationToken)
        => throw new NotImplementedException("Pending Step 3 EF Core implementation.");

    public Task<(decimal NewBalance, int TransactionId, DateTime Timestamp)> DepositAsync(int accountId, decimal amount, string source, CancellationToken cancellationToken)
        => throw new NotImplementedException("Pending Step 3 EF Core implementation.");

    public Task<(decimal TransferredAmount, decimal PenaltyApplied, DateTime ClosedAt)> CloseSavingsAccountAsync(int accountId, int destinationAccountId, decimal transferAmount, decimal earlyClosurePenalty, CancellationToken cancellationToken)
        => throw new NotImplementedException("Pending Step 3 EF Core implementation.");

    public Task<(decimal AmountWithdrawn, decimal PenaltyApplied, decimal NewBalance, DateTime ProcessedAt)> WithdrawAsync(int accountId, decimal amount, string destinationLabel, decimal earlyWithdrawalPenalty, CancellationToken cancellationToken)
        => throw new NotImplementedException("Pending Step 3 EF Core implementation.");

    public Task<AutoDeposit?> GetAutoDepositAsync(int accountId, CancellationToken cancellationToken)
        => throw new NotImplementedException("Pending Step 3 EF Core implementation.");

    public Task SaveAutoDepositAsync(AutoDeposit autoDeposit, CancellationToken cancellationToken)
        => throw new NotImplementedException("Pending Step 3 EF Core implementation.");

    public Task<IReadOnlyCollection<(int Id, string DisplayName)>> GetFundingSourcesAsync(int userId, CancellationToken cancellationToken)
        => throw new NotImplementedException("Pending Step 3 EF Core implementation.");

    public Task<(IReadOnlyCollection<SavingsTransaction> Items, int TotalCount)> GetTransactionsPagedAsync(int accountId, string typeFilter, int page, int pageSize, CancellationToken cancellationToken)
        => throw new NotImplementedException("Pending Step 3 EF Core implementation.");
}
