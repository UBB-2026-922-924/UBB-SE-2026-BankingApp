namespace BankingApp.Infrastructure.Persistence.Repositories;

using Domain.Aggregates.SavedBillerAggregate;
using BankingApp.Domain.Repositories;
using Data;
using Microsoft.EntityFrameworkCore;

public sealed class SavedBillerRepository(AppDbContext dbContext) : ISavedBillerRepository
{
    public async Task<SavedBiller?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.SavedBillers.FirstOrDefaultAsync(savedBiller => savedBiller.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SavedBiller>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SavedBillers
            .Where(savedBiller => savedBiller.UserId == userId)
            .OrderBy(savedBiller => savedBiller.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SavedBiller savedBiller, CancellationToken cancellationToken = default)
    {
        await dbContext.SavedBillers.AddAsync(savedBiller, cancellationToken);
    }

    public Task UpdateAsync(SavedBiller savedBiller, CancellationToken cancellationToken = default)
    {
        dbContext.SavedBillers.Update(savedBiller);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SavedBiller savedBiller, CancellationToken cancellationToken = default)
    {
        dbContext.SavedBillers.Remove(savedBiller);
        return Task.CompletedTask;
    }
}
