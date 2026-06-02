namespace BankingApp.Infrastructure.Persistence.Data;

using Application.Shared.Persistence;
using Domain.Common.Primitives;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = dbContext.ChangeTracker
            .Entries<AggregateRoot<int>>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (AggregateRoot<int> aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }
    }
}
