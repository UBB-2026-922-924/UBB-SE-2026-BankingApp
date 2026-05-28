namespace BankingApp.Application.Common.Logging;

using System;
using Microsoft.Extensions.Logging;

internal static partial class ApplicationLogMessages
{
    [LoggerMessage(EventId = 2000, Level = LogLevel.Warning, Message = "Login failed: user not found for email.")]
    internal static partial void LoginUserNotFoundForEmail(this ILogger logger);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Login with password rejected for OAuth-only account {UserId}.")]
    internal static partial void LoginOAuthOnlyPasswordRejected(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Error, Message = "Password hash verification threw for user {UserId}: {Error}")]
    internal static partial void PasswordHashVerificationFailed(this ILogger logger, int userId, string error);

    [LoggerMessage(EventId = 2011, Level = LogLevel.Warning, Message = "Logout failed: session not found.")]
    internal static partial void LogoutSessionNotFound(this ILogger logger);

    [LoggerMessage(EventId = 2012, Level = LogLevel.Information, Message = "User {UserId} logged out.")]
    internal static partial void UserLoggedOut(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2013, Level = LogLevel.Warning, Message = "Login blocked: account {UserId} is locked until {LockoutEnd}.")]
    internal static partial void LoginBlockedLockedAccount(this ILogger logger, int userId, DateTime? lockoutEnd);

    [LoggerMessage(EventId = 2014, Level = LogLevel.Warning, Message = "Failed login attempt for user {UserId}. Attempt {Attempt}/{Max}.")]
    internal static partial void FailedLoginAttempt(this ILogger logger, int userId, int attempt, int max);

    [LoggerMessage(EventId = 2015, Level = LogLevel.Error, Message = "Failed to lock account {UserId} after {Max} failed attempts.")]
    internal static partial void FailedToLockAccount(this ILogger logger, int userId, int max);

    [LoggerMessage(EventId = 2016, Level = LogLevel.Warning, Message = "Account {UserId} locked for {Minutes} minutes after {Max} failed attempts.")]
    internal static partial void AccountLockedTooManyAttempts(this ILogger logger, int userId, int minutes, int max);

    [LoggerMessage(EventId = 2020, Level = LogLevel.Error, Message = "Token generation failed for user {UserId}: {Error}")]
    internal static partial void TokenGenerationFailed(this ILogger logger, int userId, string error);

    [LoggerMessage(EventId = 2021, Level = LogLevel.Error, Message = "Session creation failed for user {UserId}.")]
    internal static partial void SessionCreationFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2022, Level = LogLevel.Information, Message = "User {UserId} logged in successfully.")]
    internal static partial void UserLoggedIn(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2023, Level = LogLevel.Information, Message = "Registration rejected: email already registered.")]
    internal static partial void RegistrationRejectedEmailAlreadyRegistered(this ILogger logger);

    [LoggerMessage(EventId = 2024, Level = LogLevel.Error, Message = "Database error while checking existing user: {Error}")]
    internal static partial void RegistrationExistingUserCheckFailed(this ILogger logger, string error);

    [LoggerMessage(EventId = 2025, Level = LogLevel.Error, Message = "User creation failed during registration: {Error}")]
    internal static partial void UserCreationFailedDuringRegistration(this ILogger logger, string error);

    [LoggerMessage(EventId = 2026, Level = LogLevel.Error, Message = "User creation failed during registration.")]
    internal static partial void UserCreationFailedDuringRegistration(this ILogger logger);

    [LoggerMessage(EventId = 2027, Level = LogLevel.Information, Message = "User registered successfully.")]
    internal static partial void UserRegisteredSuccessfully(this ILogger logger);

    [LoggerMessage(EventId = 2028, Level = LogLevel.Error, Message = "Hash generation failed during registration.")]
    internal static partial void RegistrationHashGenerationFailed(this ILogger logger);

    [LoggerMessage(EventId = 2029, Level = LogLevel.Error, Message = "Failed to check beneficiary duplicate for user {UserId}.")]
    internal static partial void BeneficiaryDuplicateCheckFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2030, Level = LogLevel.Error, Message = "Failed to create beneficiary for user {UserId}.")]
    internal static partial void BeneficiaryCreateFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2031, Level = LogLevel.Information, Message = "Beneficiary {BeneficiaryId} created for user {UserId}.")]
    internal static partial void BeneficiaryCreated(this ILogger logger, int beneficiaryId, int userId);

    [LoggerMessage(EventId = 2032, Level = LogLevel.Error, Message = "Failed to load beneficiaries for user {UserId} during update.")]
    internal static partial void BeneficiariesLoadForUpdateFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2043, Level = LogLevel.Warning, Message = "Account overview fetch failed: user {UserId} not found.")]
    internal static partial void AccountOverviewUserNotFound(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2044, Level = LogLevel.Error, Message = "Failed to fetch cards for user {UserId}: {Error}")]
    internal static partial void AccountOverviewFetchCardsFailed(this ILogger logger, int userId, string error);

    [LoggerMessage(EventId = 2045, Level = LogLevel.Error, Message = "Failed to fetch notification count for user {UserId}: {Error}")]
    internal static partial void AccountOverviewFetchNotificationCountFailed(this ILogger logger, int userId, string error);

    [LoggerMessage(EventId = 2046, Level = LogLevel.Error, Message = "Failed to fetch accounts for user {UserId}: {Error}")]
    internal static partial void AccountOverviewFetchAccountsFailed(this ILogger logger, int userId, string error);

    [LoggerMessage(EventId = 2047, Level = LogLevel.Error, Message = "Failed to fetch transactions for account {AccountId}: {Error}")]
    internal static partial void AccountOverviewFetchTransactionsFailed(this ILogger logger, int accountId, string error);

    [LoggerMessage(EventId = 2048, Level = LogLevel.Warning, Message = "Profile fetch failed: user {UserId} not found.")]
    internal static partial void ProfileFetchUserNotFound(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2049, Level = LogLevel.Warning, Message = "Profile update failed: user {UserId} not found.")]
    internal static partial void ProfileUpdateUserNotFound(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2050, Level = LogLevel.Error, Message = "Profile update failed for user {UserId}.")]
    internal static partial void ProfileUpdateFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2051, Level = LogLevel.Warning, Message = "Password change failed: user {UserId} not found.")]
    internal static partial void PasswordChangeUserNotFound(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2052, Level = LogLevel.Warning, Message = "Password change rejected for OAuth-only account {UserId}.")]
    internal static partial void PasswordChangeOAuthOnlyRejected(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2053, Level = LogLevel.Error, Message = "Hash verification threw during password change for user {UserId}.")]
    internal static partial void PasswordChangeHashVerificationFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2054, Level = LogLevel.Warning, Message = "Password change failed for user {UserId}: incorrect current password.")]
    internal static partial void PasswordChangeIncorrectCurrentPassword(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2055, Level = LogLevel.Error, Message = "Hash generation failed during password change for user {UserId}.")]
    internal static partial void PasswordChangeHashGenerationFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2056, Level = LogLevel.Information, Message = "Password changed successfully for user {UserId}.")]
    internal static partial void PasswordChangedSuccessfully(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2063, Level = LogLevel.Warning, Message = "Notification preferences fetch failed: user {UserId} not found.")]
    internal static partial void NotificationPreferencesFetchUserNotFound(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2064, Level = LogLevel.Error, Message = "Failed to fetch notification preferences for user {UserId}: {Error}")]
    internal static partial void NotificationPreferencesFetchFailed(this ILogger logger, int userId, string error);

    [LoggerMessage(EventId = 2065, Level = LogLevel.Warning, Message = "Notification preferences update failed: user {UserId} not found.")]
    internal static partial void NotificationPreferencesUpdateUserNotFound(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2066, Level = LogLevel.Error, Message = "Failed to update notification preferences for user {UserId}.")]
    internal static partial void NotificationPreferencesUpdateFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2067, Level = LogLevel.Warning, Message = "Password verification failed: user {UserId} not found.")]
    internal static partial void PasswordVerificationUserNotFound(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2068, Level = LogLevel.Warning, Message = "Password verification rejected for OAuth-only account {UserId}.")]
    internal static partial void PasswordVerificationOAuthOnlyRejected(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2069, Level = LogLevel.Warning, Message = "Get sessions failed: user {UserId} not found.")]
    internal static partial void GetSessionsUserNotFound(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2070, Level = LogLevel.Error, Message = "Failed to fetch sessions for user {UserId}: {Error}")]
    internal static partial void GetSessionsFailed(this ILogger logger, int userId, string error);

    [LoggerMessage(EventId = 2071, Level = LogLevel.Warning, Message = "Revoke session failed: user {UserId} not found.")]
    internal static partial void RevokeSessionUserNotFound(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2072, Level = LogLevel.Error, Message = "Failed to revoke session {SessionId} for user {UserId}.")]
    internal static partial void RevokeSessionFailed(this ILogger logger, int sessionId, int userId);

    [LoggerMessage(EventId = 2073, Level = LogLevel.Information, Message = "Session {SessionId} revoked for user {UserId}.")]
    internal static partial void SessionRevoked(this ILogger logger, int sessionId, int userId);

    [LoggerMessage(EventId = 2077, Level = LogLevel.Warning, Message = "Transfer failed: could not retrieve accounts for user {UserId}.")]
    internal static partial void TransferAccountsLookupFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2078, Level = LogLevel.Warning, Message = "Transfer failed: account {AccountId} not found for user {UserId}.")]
    internal static partial void TransferAccountNotFound(this ILogger logger, int accountId, int userId);

    [LoggerMessage(EventId = 2079, Level = LogLevel.Warning, Message = "Transfer failed: account {AccountId} is not active.")]
    internal static partial void TransferAccountNotActive(this ILogger logger, int accountId);

    [LoggerMessage(EventId = 2080, Level = LogLevel.Warning, Message = "Transfer failed: insufficient funds on account {AccountId}.")]
    internal static partial void TransferInsufficientFunds(this ILogger logger, int accountId);

    [LoggerMessage(EventId = 2081, Level = LogLevel.Error, Message = "Failed to retrieve transfer history for user {UserId}.")]
    internal static partial void TransferHistoryFetchFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2084, Level = LogLevel.Error, Message = "Transfer failed: could not debit account {AccountId}.")]
    internal static partial void TransferDebitFailed(this ILogger logger, int accountId);

    [LoggerMessage(EventId = 2085, Level = LogLevel.Error, Message = "Transfer failed: could not log transaction for account {AccountId}.")]
    internal static partial void TransferTransactionLogFailed(this ILogger logger, int accountId);

    [LoggerMessage(EventId = 2086, Level = LogLevel.Error, Message = "Transfer failed: could not persist transfer record for user {UserId}.")]
    internal static partial void TransferPersistenceFailed(this ILogger logger, int userId);

    [LoggerMessage(EventId = 2088, Level = LogLevel.Warning, Message = "Card {CardId} not found for user {UserId}.")]
    internal static partial void CardNotFound(this ILogger logger, int cardId, int userId);

    [LoggerMessage(EventId = 2089, Level = LogLevel.Information, Message = "Card {CardId} frozen for user {UserId}.")]
    internal static partial void CardFrozen(this ILogger logger, int cardId, int userId);

    [LoggerMessage(EventId = 2090, Level = LogLevel.Information, Message = "Card {CardId} unfrozen for user {UserId}.")]
    internal static partial void CardUnfrozen(this ILogger logger, int cardId, int userId);

    [LoggerMessage(EventId = 2091, Level = LogLevel.Information, Message = "Card {CardId} cancelled for user {UserId}.")]
    internal static partial void CardCancelled(this ILogger logger, int cardId, int userId);

    [LoggerMessage(EventId = 2092, Level = LogLevel.Error, Message = "Failed to retrieve cards for user {UserId}: {Error}")]
    internal static partial void GetCardsQueryFailed(this ILogger logger, int userId, string error);

    [LoggerMessage(EventId = 2093, Level = LogLevel.Warning, Message = "Issue card failed: account {AccountId} not found for user {UserId}.")]
    internal static partial void IssueCardAccountNotFound(this ILogger logger, int accountId, int userId);

    [LoggerMessage(EventId = 2094, Level = LogLevel.Information, Message = "Card {CardId} issued on account {AccountId} for user {UserId}.")]
    internal static partial void CardIssued(this ILogger logger, int cardId, int accountId, int userId);
}
