namespace BankingApp.Domain.Repositories;

using ReferenceData.Billers;

public interface IBillerRepository
{
    public Task<Biller?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<Biller>> ListActiveAsync(CancellationToken cancellationToken = default);
}
