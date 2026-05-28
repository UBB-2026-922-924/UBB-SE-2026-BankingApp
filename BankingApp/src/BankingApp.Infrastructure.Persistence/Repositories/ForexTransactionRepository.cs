namespace BankingApp.Infrastructure.Persistence.Repositories;

using Domain.Aggregates.ForexAggregate;
using BankingApp.Domain.Repositories;
using Data;
using Microsoft.EntityFrameworkCore;

public sealed class ForexTransactionRepository(AppDbContext dbContext) : IForexRepository
{
    public async Task<ForexTransaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ForexTransactions.FirstOrDefaultAsync(forexTransaction => forexTransaction.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ForexTransaction>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ForexTransactions
            .Where(forexTransaction => forexTransaction.UserId == userId)
            .OrderByDescending(forexTransaction => forexTransaction.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ForexTransaction forexTransaction, CancellationToken cancellationToken = default)
    {
        await dbContext.ForexTransactions.AddAsync(forexTransaction, cancellationToken);
    }
}
