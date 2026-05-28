namespace BankingApp.Domain.Repositories;

using Aggregates.SavedBillerAggregate;

public interface ISavedBillerRepository
{
    public Task<SavedBiller?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<SavedBiller>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    public Task AddAsync(SavedBiller savedBiller, CancellationToken cancellationToken = default);
    public Task UpdateAsync(SavedBiller savedBiller, CancellationToken cancellationToken = default);
    public Task DeleteAsync(SavedBiller savedBiller, CancellationToken cancellationToken = default);
}
