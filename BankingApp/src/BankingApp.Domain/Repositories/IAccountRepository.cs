namespace BankingApp.Domain.Repositories;

using Aggregates.AccountAggregate;

public interface IAccountRepository
{
    public Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<Account?> GetByIbanAsync(string iban, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<Account>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    public Task AddAsync(Account account, CancellationToken cancellationToken = default);
    public Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
}
