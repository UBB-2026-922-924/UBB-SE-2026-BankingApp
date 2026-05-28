namespace BankingApp.Domain.Repositories;

using Aggregates.UserAggregate;
using ValueObjects;

public interface IUserRepository
{
    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    public Task AddAsync(User user, CancellationToken cancellationToken = default);
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
