namespace BankingApp.Domain.Repositories;

using Aggregates.TransferAggregate;

public interface ITransferRepository
{
    public Task<Transfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<Transfer>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    public Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default);
}
