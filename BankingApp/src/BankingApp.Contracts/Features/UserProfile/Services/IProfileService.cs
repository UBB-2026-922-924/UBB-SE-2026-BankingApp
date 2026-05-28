namespace BankingApp.Contracts.Features.UserProfile.Services;

using Dtos;
using ErrorOr;

public interface IProfileService
{
    public Task<ErrorOr<ProfileDto>> GetProfileAsync(CancellationToken ct = default);
    public Task<ErrorOr<Success>> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken ct = default);
    public Task<ErrorOr<bool>> VerifyPasswordAsync(string password, CancellationToken ct = default);
    public Task<ErrorOr<Success>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default);
    public Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync(CancellationToken ct = default);
    public Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(List<NotificationPreferenceDto> preferences, CancellationToken ct = default);
    public Task<ErrorOr<List<SessionDto>>> GetSessionsAsync(CancellationToken ct = default);
    public Task<ErrorOr<Success>> RevokeSessionAsync(int sessionId, CancellationToken ct = default);
}
