namespace BankingApp.Domain.Repositories;

using Aggregates.IdentityAggregate;

public interface IIdentityRepository
{
    public Task<IdentityAccount?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<IdentityAccount?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    public Task<IdentityAccount?> GetBySessionTokenAsync(string token, CancellationToken cancellationToken = default);
    public Task AddAsync(IdentityAccount identityAccount, CancellationToken cancellationToken = default);
    public Task UpdateAsync(IdentityAccount identityAccount, CancellationToken cancellationToken = default);
}
