namespace BankingApp.Infrastructure.Http.Features.Billers.Services;

using Application.Shared.Http;
using Contracts.Features.Billers.Dtos;
using Contracts.Features.Billers.Services;
using Contracts.Http;
using ErrorOr;

public sealed class BillerService(IApiClient apiClient) : IBillerService
{
    public Task<ErrorOr<List<BillerDto>>> GetBillersAsync(string? search = null, string? category = null, CancellationToken ct = default)
    {
        string endpoint = ApiEndpoints.Billers.Base;
        string separator = "?";

        if (!string.IsNullOrWhiteSpace(search))
        {
            endpoint += $"{separator}search={Uri.EscapeDataString(search)}";
            separator = "&";
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            endpoint += $"{separator}category={Uri.EscapeDataString(category)}";
        }

        return apiClient.GetAsync<List<BillerDto>>(endpoint, ct);
    }

    public Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<SavedBillerDto>>(ApiEndpoints.Billers.SavedFull, ct);

    public Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(SaveBillerRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<SaveBillerRequest, SavedBillerDto>(ApiEndpoints.Billers.SavedFull, request, ct);

    public Task<ErrorOr<Success>> DeleteSavedBillerAsync(int savedBillerId, CancellationToken ct = default)
        => apiClient.DeleteAsync(ApiEndpoints.Billers.SavedByIdFull(savedBillerId), ct);
}
