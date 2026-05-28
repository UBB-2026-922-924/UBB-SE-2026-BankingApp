namespace BankingApp.Application.Shared.Persistence;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
