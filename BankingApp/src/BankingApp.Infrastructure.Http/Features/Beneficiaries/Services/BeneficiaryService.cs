namespace BankingApp.Infrastructure.Http.Features.Beneficiaries.Services;

using Application.Shared.Http;
using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Features.Beneficiaries.Services;
using Contracts.Http;
using ErrorOr;

public sealed class BeneficiaryService(IApiClient apiClient) : IBeneficiaryService
{
    public Task<ErrorOr<List<BeneficiaryDto>>> GetAllAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<BeneficiaryDto>>(ApiEndpoints.Beneficiaries.Base, ct);

    public async Task<ErrorOr<BeneficiaryDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        ErrorOr<List<BeneficiaryDto>> result = await GetAllAsync(ct);
        if (result.IsError)
        {
            return result.Errors;
        }

        BeneficiaryDto? beneficiary = result.Value.FirstOrDefault(beneficiaryDto => beneficiaryDto.Id == id);

        if (beneficiary is null)
        {
            return Error.NotFound(
                code: "Beneficiary.NotFound",
                description: $"Beneficiary {id} was not found.");
        }

        return beneficiary;
    }

    public Task<ErrorOr<Success>> CreateAsync(CreateBeneficiaryRequest request, CancellationToken ct = default)
        => apiClient.PostAsync(ApiEndpoints.Beneficiaries.Base, request, ct);

    public Task<ErrorOr<Success>> UpdateAsync(int id, UpdateBeneficiaryRequest request, CancellationToken ct = default)
        => apiClient.PutAsync(ApiEndpoints.Beneficiaries.ByIdFull(id), request, ct);

    public Task<ErrorOr<Success>> DeleteAsync(int id, CancellationToken ct = default)
        => apiClient.DeleteAsync(ApiEndpoints.Beneficiaries.ByIdFull(id), ct);
}
