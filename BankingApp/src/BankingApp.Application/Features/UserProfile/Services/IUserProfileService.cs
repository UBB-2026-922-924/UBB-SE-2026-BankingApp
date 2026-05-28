namespace BankingApp.Application.Features.UserProfile.Services;

using Contracts.Features.UserProfile.Dtos;
using ErrorOr;

public interface IUserProfileService
{
    public Task<ErrorOr<ProfileDto>> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> UpdateProfileAsync(int userId, string? fullName, string? phoneNumber, DateTime? dateOfBirth, string? address, string? nationality, string? preferredLanguage, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    public Task<ErrorOr<bool>> VerifyPasswordAsync(int userId, string password, CancellationToken cancellationToken = default);
    public Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(int userId, List<NotificationPreferenceDto> preferences, CancellationToken cancellationToken = default);
    public Task<ErrorOr<List<SessionDto>>> GetActiveSessionsAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> RevokeSessionAsync(int userId, int sessionId, CancellationToken cancellationToken = default);
}
