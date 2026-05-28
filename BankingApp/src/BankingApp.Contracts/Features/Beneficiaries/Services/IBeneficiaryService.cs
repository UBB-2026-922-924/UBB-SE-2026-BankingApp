namespace BankingApp.Contracts.Features.Beneficiaries.Services;

using Dtos;
using ErrorOr;

public interface IBeneficiaryService
{
    public Task<ErrorOr<List<BeneficiaryDto>>> GetAllAsync(CancellationToken ct = default);
    public Task<ErrorOr<BeneficiaryDto>> GetByIdAsync(int id, CancellationToken ct = default);
    public Task<ErrorOr<Success>> CreateAsync(CreateBeneficiaryRequest request, CancellationToken ct = default);
    public Task<ErrorOr<Success>> UpdateAsync(int id, UpdateBeneficiaryRequest request, CancellationToken ct = default);
    public Task<ErrorOr<Success>> DeleteAsync(int id, CancellationToken ct = default);
}
