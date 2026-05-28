namespace BankingApp.Infrastructure.Http.Features.Cards.Services;

using Application.Shared.Http;
using Contracts.Features.Cards.Dtos;
using Contracts.Features.Cards.Services;
using Contracts.Http;
using ErrorOr;

public sealed class CardService(IApiClient apiClient) : ICardService
{
    public Task<ErrorOr<List<CardDetailsDto>>> GetCardsAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<CardDetailsDto>>(ApiEndpoints.Cards.Base, ct);

    public Task<ErrorOr<CardDetailsDto>> IssueCardAsync(IssueCardRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<IssueCardRequest, CardDetailsDto>(ApiEndpoints.Cards.Base, request, ct);

    public Task<ErrorOr<Success>> FreezeCardAsync(int id, CancellationToken ct = default)
        => apiClient.PutAsync(ApiEndpoints.Cards.FreezeFull(id), new { }, ct);

    public Task<ErrorOr<Success>> UnfreezeCardAsync(int id, CancellationToken ct = default)
        => apiClient.PutAsync(ApiEndpoints.Cards.UnfreezeFull(id), new { }, ct);

    public Task<ErrorOr<Success>> CancelCardAsync(int id, CancellationToken ct = default)
        => apiClient.DeleteAsync(ApiEndpoints.Cards.ByIdFull(id), ct);
}
