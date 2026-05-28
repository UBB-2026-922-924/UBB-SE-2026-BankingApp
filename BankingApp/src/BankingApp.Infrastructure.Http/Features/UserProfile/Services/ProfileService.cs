namespace BankingApp.Infrastructure.Http.Features.UserProfile.Services;

using Application.Shared.Http;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using Contracts.Http;
using ErrorOr;

public sealed class ProfileService(IApiClient apiClient) : IProfileService
{
    public Task<ErrorOr<ProfileDto>> GetProfileAsync(CancellationToken ct = default)
        => apiClient.GetAsync<ProfileDto>(ApiEndpoints.Profile.Base, ct);

    public Task<ErrorOr<Success>> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken ct = default)
        => apiClient.PutAsync(ApiEndpoints.Profile.Base, request, ct);

    public Task<ErrorOr<bool>> VerifyPasswordAsync(string password, CancellationToken ct = default)
        => apiClient.PostAsync<string, bool>(ApiEndpoints.Profile.VerifyPasswordFull, password, ct);

    public Task<ErrorOr<Success>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
        => apiClient.PutAsync(ApiEndpoints.Profile.ChangePasswordFull, request, ct);

    public Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<NotificationPreferenceDto>>(ApiEndpoints.Profile.NotificationPreferencesFull, ct);

    public Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(List<NotificationPreferenceDto> preferences, CancellationToken ct = default)
        => apiClient.PutAsync(ApiEndpoints.Profile.NotificationPreferencesFull, preferences, ct);

    public Task<ErrorOr<List<SessionDto>>> GetSessionsAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<SessionDto>>(ApiEndpoints.Profile.SessionsFull, ct);

    public Task<ErrorOr<Success>> RevokeSessionAsync(int sessionId, CancellationToken ct = default)
        => apiClient.DeleteAsync(ApiEndpoints.Profile.SessionByIdFull(sessionId), ct);
}
