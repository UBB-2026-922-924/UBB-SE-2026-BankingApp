namespace BankingApp.Infrastructure.Persistence.Repositories;

using Domain.Aggregates.AccountAggregate.Entities;
using BankingApp.Domain.Repositories;
using Data;
using Microsoft.EntityFrameworkCore;

public sealed class TransactionRepository(AppDbContext dbContext) : ITransactionRepository
{
    public async Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Accounts
            .AsNoTracking()
            .SelectMany(account => account.Transactions)
            .FirstOrDefaultAsync(transaction => transaction.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Transaction>> ListByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.Id == accountId)
            .SelectMany(account => account.Transactions)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
