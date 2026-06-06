namespace BankingApp.Contracts.Features.Billers.Services;

using Dtos;
using ErrorOr;

public interface IBillerService
{
    public Task<ErrorOr<List<BillerDto>>> GetBillersAsync(string? search = null, string? category = null, CancellationToken ct = default);
    public Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync(CancellationToken ct = default);
    public Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(SaveBillerRequest request, CancellationToken ct = default);
    public Task<ErrorOr<Success>> DeleteSavedBillerAsync(int savedBillerId, CancellationToken ct = default);
}
