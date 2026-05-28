namespace BankingApp.Application.Features.UserProfile.Services;

using Common.Security;
using Common.Validation;
using Contracts.Features.UserProfile.Dtos;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed class UserProfileService(
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    IHashService hashService,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<UserProfileService> logger)
    : IUserProfileService
{
    public async Task<ErrorOr<ProfileDto>> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.ProfileFetchUserNotFound(logger, userId);
            return UserErrors.NotFound;
        }

        return new ProfileDto
        {
            UserId = user.Id,
            Email = user.Email.Value,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            Nationality = user.Nationality,
            PreferredLanguage = user.PreferredLanguage
        };
    }

    public async Task<ErrorOr<Success>> UpdateProfileAsync(
        int userId, string? fullName, string? phoneNumber, DateTime? dateOfBirth,
        string? address, string? nationality, string? preferredLanguage, CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.ProfileUpdateUserNotFound(logger, userId);
            return UserErrors.NotFound;
        }

        string resolvedFullName = fullName?.Trim() ?? user.FullName;
        if (string.IsNullOrWhiteSpace(resolvedFullName))
        {
            return ProfileErrors.FullNameRequired;
        }

        if (phoneNumber is not null && !InputRules.IsValidPhoneNumber(phoneNumber))
        {
            return ProfileErrors.InvalidPhone;
        }

        string resolvedLanguage = preferredLanguage?.Trim() ?? user.PreferredLanguage;
        if (string.IsNullOrWhiteSpace(resolvedLanguage))
        {
            return ProfileErrors.PreferredLanguageRequired;
        }

        user.UpdateProfile(
            resolvedFullName,
            phoneNumber ?? user.PhoneNumber,
            dateOfBirth ?? user.DateOfBirth,
            address?.Trim() ?? user.Address,
            nationality?.Trim() ?? user.Nationality,
            resolvedLanguage,
            clock.UtcNow);

        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.PasswordChangeUserNotFound(logger, userId);
            return UserErrors.NotFound;
        }

        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (identity is null)
        {
            return UserErrors.NotFound;
        }

        if (identity.PasswordHash is null)
        {
            ApplicationLogMessages.PasswordChangeOAuthOnlyRejected(logger, user.Id);
            return ProfileErrors.IncorrectPassword;
        }

        ErrorOr<bool> verifyResult = hashService.Verify(currentPassword, identity.PasswordHash.Value);
        if (verifyResult.IsError)
        {
            ApplicationLogMessages.PasswordChangeHashVerificationFailed(logger, user.Id);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            ApplicationLogMessages.PasswordChangeIncorrectCurrentPassword(logger, user.Id);
            return ProfileErrors.IncorrectPassword;
        }

        ErrorOr<string> newHashResult = hashService.GetHash(newPassword);
        if (newHashResult.IsError)
        {
            ApplicationLogMessages.PasswordChangeHashGenerationFailed(logger, user.Id);
            return newHashResult.FirstError;
        }

        identity.UpdatePassword(HashedPassword.Wrap(newHashResult.Value));
        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.PasswordChangedSuccessfully(logger, user.Id);
        return Result.Success;
    }

    public async Task<ErrorOr<bool>> VerifyPasswordAsync(int userId, string password, CancellationToken cancellationToken = default)
    {
        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(userId, cancellationToken);
        if (identity is null)
        {
            ApplicationLogMessages.PasswordChangeUserNotFound(logger, userId);
            return UserErrors.NotFound;
        }

        if (identity.PasswordHash is null)
        {
            return false;
        }

        ErrorOr<bool> verifyResult = hashService.Verify(password, identity.PasswordHash.Value);
        if (verifyResult.IsError)
        {
            ApplicationLogMessages.PasswordChangeHashVerificationFailed(logger, userId);
            return verifyResult.FirstError;
        }

        return verifyResult.Value;
    }

    public async Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync(int userId, CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.NotificationPreferencesFetchUserNotFound(logger, userId);
            return UserErrors.NotFound;
        }

        return user.NotificationPreferences
            .Select(notificationPreference => new NotificationPreferenceDto
            {
                Id = notificationPreference.Id,
                UserId = notificationPreference.UserId,
                Category = notificationPreference.Category,
                PushEnabled = notificationPreference.PushEnabled,
                EmailEnabled = notificationPreference.EmailEnabled,
                SmsEnabled = notificationPreference.SmsEnabled,
                MinAmountThreshold = notificationPreference.MinAmountThreshold
            })
            .ToList();
    }

    public async Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(int userId, List<NotificationPreferenceDto> preferences, CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.NotificationPreferencesUpdateUserNotFound(logger, userId);
            return UserErrors.NotFound;
        }

        foreach (NotificationPreferenceDto pref in preferences)
        {
            user.SetNotificationPreference(
                pref.Category,
                pref.PushEnabled,
                pref.EmailEnabled,
                pref.SmsEnabled,
                pref.MinAmountThreshold);
        }

        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<List<SessionDto>>> GetActiveSessionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(userId, cancellationToken);
        if (identity is null)
        {
            ApplicationLogMessages.GetSessionsUserNotFound(logger, userId);
            return UserErrors.NotFound;
        }

        return identity.Sessions
            .Where(session => !session.IsRevoked)
            .Select(session => new SessionDto
            {
                Id = session.Id,
                DeviceInfo = session.DeviceInfo,
                Browser = session.Browser,
                IpAddress = session.IpAddress,
                LastActiveAt = session.LastActiveAt,
                CreatedAt = session.CreatedAt
            })
            .ToList();
    }

    public async Task<ErrorOr<Success>> RevokeSessionAsync(int userId, int sessionId, CancellationToken cancellationToken = default)
    {
        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(userId, cancellationToken);
        if (identity is null)
        {
            ApplicationLogMessages.RevokeSessionUserNotFound(logger, userId);
            return UserErrors.NotFound;
        }

        if (!identity.TryRevokeSession(sessionId))
        {
            ApplicationLogMessages.RevokeSessionFailed(logger, sessionId, userId);
            return AuthErrors.SessionNotFound;
        }

        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.SessionRevoked(logger, sessionId, userId);
        return Result.Success;
    }
}
