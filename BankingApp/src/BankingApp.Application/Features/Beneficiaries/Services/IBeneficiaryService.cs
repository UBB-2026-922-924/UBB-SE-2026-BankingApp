namespace BankingApp.Application.Features.Beneficiaries.Services;

using Contracts.Features.Beneficiaries.Dtos;
using ErrorOr;

public interface IBeneficiaryService
{
    public Task<ErrorOr<List<BeneficiaryDto>>> GetAllAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<BeneficiaryDto>> CreateAsync(int userId, string name, string iban, string? bankName, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> UpdateAsync(int userId, int beneficiaryId, string name, string iban, string? bankName, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> DeleteAsync(int userId, int beneficiaryId, CancellationToken cancellationToken = default);
}
