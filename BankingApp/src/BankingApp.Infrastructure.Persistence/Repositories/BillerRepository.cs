namespace BankingApp.Infrastructure.Persistence.Repositories;

using Domain.ReferenceData.Billers;
using BankingApp.Domain.Repositories;
using Data;
using Microsoft.EntityFrameworkCore;

public sealed class BillerRepository(AppDbContext dbContext) : IBillerRepository
{
    public async Task<Biller?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Billers.FirstOrDefaultAsync(biller => biller.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Biller>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Billers
            .Where(biller => biller.IsActive)
            .OrderBy(biller => biller.Name)
            .ToListAsync(cancellationToken);
    }
}
