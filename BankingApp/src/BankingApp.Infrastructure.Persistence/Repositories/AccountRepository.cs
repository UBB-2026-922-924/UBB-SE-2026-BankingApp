namespace BankingApp.Infrastructure.Persistence.Repositories;

using Domain.Aggregates.AccountAggregate;
using Domain.Repositories;
using Domain.ValueObjects;
using Data;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

public sealed class AccountRepository(AppDbContext dbContext) : IAccountRepository
{
    public async Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Accounts
            .Include(account => account.Cards)
            .Include(account => account.Transactions)
            .FirstOrDefaultAsync(account => account.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByIbanAsync(string iban, CancellationToken cancellationToken = default)
    {
        ErrorOr<Iban> ibanResult = Iban.Create(iban);
        if (ibanResult.IsError)
        {
            return null;
        }

        return await dbContext.Accounts
            .Include(account => account.Cards)
            .Include(account => account.Transactions)
            .FirstOrDefaultAsync(account => account.Iban == ibanResult.Value, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Account>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Accounts
            .Where(account => account.UserId == userId)
            .Include(account => account.Cards)
            .OrderBy(account => account.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await dbContext.Accounts.AddAsync(account, cancellationToken);
    }

    public Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        dbContext.Accounts.Update(account);
        return Task.CompletedTask;
    }
}
