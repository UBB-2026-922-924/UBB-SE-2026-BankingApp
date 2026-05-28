namespace BankingApp.Infrastructure.Persistence.Repositories;

using Domain.Aggregates.BeneficiaryAggregate;
using BankingApp.Domain.Repositories;
using Data;
using Microsoft.EntityFrameworkCore;

public sealed class BeneficiaryRepository(AppDbContext dbContext) : IBeneficiaryRepository
{
    public async Task<Beneficiary?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Beneficiaries.FirstOrDefaultAsync(beneficiary => beneficiary.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Beneficiary>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Beneficiaries
            .Where(beneficiary => beneficiary.UserId == userId)
            .OrderBy(beneficiary => beneficiary.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Beneficiary beneficiary, CancellationToken cancellationToken = default)
    {
        await dbContext.Beneficiaries.AddAsync(beneficiary, cancellationToken);
    }

    public Task UpdateAsync(Beneficiary beneficiary, CancellationToken cancellationToken = default)
    {
        dbContext.Beneficiaries.Update(beneficiary);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Beneficiary beneficiary, CancellationToken cancellationToken = default)
    {
        dbContext.Beneficiaries.Remove(beneficiary);
        return Task.CompletedTask;
    }
}
