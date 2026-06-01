namespace BankingApp.Infrastructure.Persistence.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Domain.Aggregates.SavingsAggregate;
using Domain.Aggregates.SavingsAggregate.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class SavingsRepository(AppDbContext dbContext) : ISavingsRepository
{
    private const string ClosedStatus = "Closed";

    public async Task<SavingsAccount> CreateSavingsAccountAsync(SavingsAccount account, CancellationToken cancellationToken)
    {
        dbContext.SavingsAccounts.Add(account);
        await dbContext.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<IReadOnlyCollection<SavingsAccount>> GetSavingsAccountsByUserIdAsync(int userId, bool includesClosedAccounts, CancellationToken cancellationToken)
    {
        IQueryable<SavingsAccount> query = dbContext.SavingsAccounts
            .AsNoTracking()
            .Where(a => a.UserId == userId);

        if (!includesClosedAccounts)
        {
            query = query.Where(a => a.AccountStatus != ClosedStatus);
        }

        return await query
            .OrderByDescending(a => a.Balance)
            .ToListAsync(cancellationToken);
    }

    public async Task<(decimal NewBalance, int TransactionId, DateTime Timestamp)> DepositAsync(int accountId, decimal amount, string source, CancellationToken cancellationToken)
    {
        await using IDbContextTransaction tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            SavingsAccount account = await dbContext.SavingsAccounts
                .FirstAsync(a => a.Id == accountId, cancellationToken);

            var depositResult = account.Deposit(amount);
            if (depositResult.IsError)
            {
                await tx.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(depositResult.FirstError.Description);
            }

            SavingsTransaction savingsTransaction = SavingsTransaction.Create(
                accountId, amount, SavingsTransactionType.Deposit, source, accountId, account.Balance, DateTime.UtcNow);
            dbContext.SavingsTransactions.Add(savingsTransaction);

            await dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return (account.Balance, savingsTransaction.Id, savingsTransaction.CreatedAt);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<(decimal TransferredAmount, decimal PenaltyApplied, DateTime ClosedAt)> CloseSavingsAccountAsync(
        int accountId,
        int destinationAccountId,
        decimal transferAmount,
        decimal earlyClosurePenalty,
        CancellationToken cancellationToken)
    {
        await using IDbContextTransaction tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            SavingsAccount source = await dbContext.SavingsAccounts.FirstAsync(a => a.Id == accountId, cancellationToken);
            SavingsAccount destination = await dbContext.SavingsAccounts.FirstAsync(a => a.Id == destinationAccountId, cancellationToken);

            var closeResult = source.Close();
            if (closeResult.IsError)
            {
                await tx.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(closeResult.FirstError.Description);
            }

            var depositResult = destination.Deposit(transferAmount);
            if (depositResult.IsError)
            {
                await tx.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(depositResult.FirstError.Description);
            }

            DateTime closedAt = DateTime.UtcNow;
            dbContext.SavingsTransactions.Add(SavingsTransaction.Create(
                accountId, transferAmount, SavingsTransactionType.Withdrawal, "Closure", destinationAccountId, 0m, closedAt, "Account closed"));

            await dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return (transferAmount, earlyClosurePenalty, closedAt);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<(decimal AmountWithdrawn, decimal PenaltyApplied, decimal NewBalance, DateTime ProcessedAt)> WithdrawAsync(
        int accountId,
        decimal amount,
        string destinationLabel,
        decimal earlyWithdrawalPenalty,
        CancellationToken cancellationToken)
    {
        await using IDbContextTransaction tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            SavingsAccount account = await dbContext.SavingsAccounts.FirstAsync(a => a.Id == accountId, cancellationToken);

            var withdrawResult = account.Withdraw(amount);
            if (withdrawResult.IsError)
            {
                await tx.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(withdrawResult.FirstError.Description);
            }

            DateTime processedAt = DateTime.UtcNow;
            dbContext.SavingsTransactions.Add(SavingsTransaction.Create(
                accountId, amount, SavingsTransactionType.Withdrawal, "Manual", accountId, account.Balance, processedAt, $"To: {destinationLabel}"));

            await dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return (amount, earlyWithdrawalPenalty, account.Balance, processedAt);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<AutoDeposit?> GetAutoDepositAsync(int accountId, CancellationToken cancellationToken)
    {
        return await dbContext.AutoDeposits
            .AsNoTracking()
            .Where(d => d.SavingsAccountId == accountId)
            .OrderByDescending(d => d.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveAutoDepositAsync(AutoDeposit autoDeposit, CancellationToken cancellationToken)
    {
        AutoDeposit? existing = await dbContext.AutoDeposits
            .FirstOrDefaultAsync(d => d.Id == autoDeposit.Id, cancellationToken);

        if (existing is null)
        {
            dbContext.AutoDeposits.Add(autoDeposit);
        }
        else
        {
            dbContext.AutoDeposits.Update(autoDeposit);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<(int Id, string DisplayName)>> GetFundingSourcesAsync(int userId, CancellationToken cancellationToken)
    {
        return await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Status != ClosedStatus)
            .OrderBy(a => a.Id)
            .Select(a => ValueTuple.Create(
                a.Id,
                (a.AccountName ?? a.AccountType) + " •" + a.Iban.Value.Substring(a.Iban.Value.Length - 4)))
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyCollection<SavingsTransaction> Items, int TotalCount)> GetTransactionsPagedAsync(
        int accountId,
        string typeFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<SavingsTransaction> query = dbContext.SavingsTransactions
            .AsNoTracking()
            .Where(t => t.SavingsAccountId == accountId);

        if (!string.IsNullOrEmpty(typeFilter) && typeFilter != "All" && Enum.TryParse<SavingsTransactionType>(typeFilter, out SavingsTransactionType parsedType))
        {
            query = query.Where(t => t.Type == parsedType);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        List<SavingsTransaction> items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
