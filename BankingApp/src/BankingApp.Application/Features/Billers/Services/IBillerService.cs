namespace BankingApp.Application.Features.Billers.Services;

using Contracts.Features.Billers.Dtos;
using ErrorOr;

public interface IBillerService
{
    public Task<ErrorOr<List<BillerDto>>> GetBillersAsync(string? search = null, string? category = null, CancellationToken cancellationToken = default);
    public Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(int userId, int billerId, string? nickname, string? defaultReference, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> DeleteSavedBillerAsync(int userId, int savedBillerId, CancellationToken cancellationToken = default);
}
