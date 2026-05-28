namespace BankingApp.Infrastructure.Persistence.Repositories;

using Domain.Aggregates.IdentityAggregate;
using BankingApp.Domain.Repositories;
using Data;
using Microsoft.EntityFrameworkCore;

public sealed class IdentityRepository(AppDbContext dbContext) : IIdentityRepository
{
    public async Task<IdentityAccount?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(identityAccount => identityAccount.Id == id, cancellationToken);
    }

    public async Task<IdentityAccount?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(identityAccount => identityAccount.UserId == userId, cancellationToken);
    }

    public async Task<IdentityAccount?> GetBySessionTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(identityAccount => identityAccount.Sessions.Any(session => session.Token == token), cancellationToken);
    }

    public async Task AddAsync(IdentityAccount identityAccount, CancellationToken cancellationToken = default)
    {
        await dbContext.IdentityAccounts.AddAsync(identityAccount, cancellationToken);
    }

    public Task UpdateAsync(IdentityAccount identityAccount, CancellationToken cancellationToken = default)
    {
        dbContext.IdentityAccounts.Update(identityAccount);
        return Task.CompletedTask;
    }

    private IQueryable<IdentityAccount> Query() => dbContext.IdentityAccounts
        .Include(identityAccount => identityAccount.Sessions);
}
