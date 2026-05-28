namespace BankingApp.Domain.Repositories;

using Aggregates.BeneficiaryAggregate;

public interface IBeneficiaryRepository
{
    public Task<Beneficiary?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<Beneficiary>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    public Task AddAsync(Beneficiary beneficiary, CancellationToken cancellationToken = default);
    public Task UpdateAsync(Beneficiary beneficiary, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Beneficiary beneficiary, CancellationToken cancellationToken = default);
}
