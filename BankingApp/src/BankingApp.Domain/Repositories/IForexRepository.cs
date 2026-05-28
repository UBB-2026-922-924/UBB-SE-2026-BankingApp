namespace BankingApp.Domain.Repositories;

using Aggregates.ForexAggregate;

public interface IForexRepository
{
    public Task<ForexTransaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<ForexTransaction>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    public Task AddAsync(ForexTransaction forexTransaction, CancellationToken cancellationToken = default);
}
