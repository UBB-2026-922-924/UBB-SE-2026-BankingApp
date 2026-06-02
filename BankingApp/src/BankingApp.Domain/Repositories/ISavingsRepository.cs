namespace BankingApp.Domain.Repositories;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aggregates.SavingsAggregate;
using Aggregates.SavingsAggregate.Entities;

/// <summary>Defines persistence operations for savings accounts and transactions.</summary>
public interface ISavingsRepository
{
    /// <summary>Persists a new savings account and returns it with its assigned identifier.</summary>
    public Task<SavingsAccount> CreateSavingsAccountAsync(SavingsAccount account, CancellationToken cancellationToken);

    /// <summary>Gets savings accounts for a user.</summary>
    public Task<IReadOnlyCollection<SavingsAccount>> GetSavingsAccountsByUserIdAsync(int userId, bool includesClosedAccounts, CancellationToken cancellationToken);

    /// <summary>Deposits funds into a savings account. Returns (newBalance, transactionId, timestamp).</summary>
    public Task<(decimal NewBalance, int TransactionId, DateTime Timestamp)> DepositAsync(int accountId, decimal amount, string source, CancellationToken cancellationToken);

    /// <summary>Closes a savings account and transfers remaining funds. Returns (transferredAmount, penaltyApplied, closedAt).</summary>
    public Task<(decimal TransferredAmount, decimal PenaltyApplied, DateTime ClosedAt)> CloseSavingsAccountAsync(
        int accountId,
        int destinationAccountId,
        decimal transferAmount,
        decimal earlyClosurePenalty,
        CancellationToken cancellationToken);

    /// <summary>Withdraws funds from a savings account. Returns (amountWithdrawn, penaltyApplied, newBalance, processedAt).</summary>
    public Task<(decimal AmountWithdrawn, decimal PenaltyApplied, decimal NewBalance, DateTime ProcessedAt)> WithdrawAsync(
        int accountId,
        decimal amount,
        string destinationLabel,
        decimal earlyWithdrawalPenalty,
        CancellationToken cancellationToken);

    /// <summary>Gets recurring auto-deposit settings for an account.</summary>
    public Task<AutoDeposit?> GetAutoDepositAsync(int accountId, CancellationToken cancellationToken);

    /// <summary>Creates or updates auto-deposit settings.</summary>
    public Task SaveAutoDepositAsync(AutoDeposit autoDeposit, CancellationToken cancellationToken);

    /// <summary>Gets available funding sources as (id, displayName) pairs.</summary>
    public Task<IReadOnlyCollection<(int Id, string DisplayName)>> GetFundingSourcesAsync(int userId, CancellationToken cancellationToken);

    /// <summary>Gets paged transaction history and total count.</summary>
    public Task<(IReadOnlyCollection<SavingsTransaction> Items, int TotalCount)> GetTransactionsPagedAsync(
        int accountId,
        string typeFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
