namespace BankingApp.Infrastructure.Persistence.Repositories;

using Domain.Aggregates.TransferAggregate;
using BankingApp.Domain.Repositories;
using Data;
using Microsoft.EntityFrameworkCore;

public sealed class TransferRepository(AppDbContext dbContext) : ITransferRepository
{
    public async Task<Transfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Transfers.FirstOrDefaultAsync(transfer => transfer.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Transfer>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Transfers
            .Where(transfer => transfer.UserId == userId)
            .OrderByDescending(transfer => transfer.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default)
    {
        await dbContext.Transfers.AddAsync(transfer, cancellationToken);
    }
}
