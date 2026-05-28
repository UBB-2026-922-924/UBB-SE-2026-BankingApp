namespace BankingApp.Desktop.Logging;

using System;
using Microsoft.Extensions.Logging;

/// <summary>Centralized <see cref="LoggerMessageAttribute"/> definitions for Desktop logging.</summary>
internal static partial class DesktopLogMessages
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Critical,
        Message = "ApiBaseUrl is missing from configuration. The client cannot connect to the server.")]
    internal static partial void ApiBaseUrlMissing(this ILogger logger);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Error, Message = "Failed to load beneficiaries")]
    internal static partial void FailedToLoadBeneficiaries(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Error, Message = "Failed to delete beneficiary {Id}")]
    internal static partial void FailedToDeleteBeneficiary(this ILogger logger, Exception exception, int id);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Error, Message = "Failed to add beneficiary")]
    internal static partial void FailedToAddBeneficiary(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Error,
        Message = "LoadNotificationPreferences: request failed: {Errors}")]
    internal static partial void LoadNotificationPreferencesFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Error,
        Message = "UpdateNotificationPreferences failed: {Errors}")]
    internal static partial void UpdateNotificationPreferencesFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Critical,
        Message = "ApiClient is not configured; login is unavailable. Error count: {ErrorCount}")]
    internal static partial void LoginUnavailableApiClientNotConfigured(this ILogger logger, int errorCount);

    [LoggerMessage(EventId = 1007, Level = LogLevel.Error, Message = "Login failed: {Errors}")]
    internal static partial void LoginFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1008, Level = LogLevel.Error, Message = "Rate preview failed: {Errors}")]
    internal static partial void RatePreviewFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1009, Level = LogLevel.Error, Message = "Rate preview failed unexpectedly")]
    internal static partial void RatePreviewFailedUnexpected(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1010, Level = LogLevel.Error, Message = "Exchange execution failed: {Errors}")]
    internal static partial void ExchangeExecutionFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1011, Level = LogLevel.Error, Message = "Exchange execution failed unexpectedly")]
    internal static partial void ExchangeExecutionFailedUnexpected(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1012, Level = LogLevel.Error, Message = "Load alerts failed: {Errors}")]
    internal static partial void LoadAlertsFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1013, Level = LogLevel.Error, Message = "Load alerts failed unexpectedly")]
    internal static partial void LoadAlertsFailedUnexpected(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1014, Level = LogLevel.Error, Message = "Create alert failed: {Errors}")]
    internal static partial void CreateAlertFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1015, Level = LogLevel.Error, Message = "Create alert failed unexpectedly")]
    internal static partial void CreateAlertFailedUnexpected(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1016, Level = LogLevel.Error, Message = "Delete alert failed: {Errors}")]
    internal static partial void DeleteAlertFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1017, Level = LogLevel.Error, Message = "Delete alert failed unexpectedly")]
    internal static partial void DeleteAlertFailedUnexpected(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1018, Level = LogLevel.Error,
        Message = "LoadProfile: profile request failed: {Errors}")]
    internal static partial void LoadProfileFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1019, Level = LogLevel.Error, Message = "UpdatePersonalInfo failed: {Errors}")]
    internal static partial void UpdatePersonalInfoFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1020, Level = LogLevel.Error, Message = "VerifyPassword failed: {Errors}")]
    internal static partial void VerifyPasswordFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1021, Level = LogLevel.Error, Message = "LoadDashboard failed: {Errors}")]
    internal static partial void LoadDashboardFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1024, Level = LogLevel.Error, Message = "ChangePassword failed: {Errors}")]
    internal static partial void ChangePasswordFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1027, Level = LogLevel.Error,
        Message = "Unexpected error loading transfer history.")]
    internal static partial void LoadTransferHistoryFailedUnexpected(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1028, Level = LogLevel.Error,
        Message = "Failed to load sessions for user {UserId}")]
    internal static partial void LoadSessionsFailed(this ILogger logger, Exception exception, int userId);

    [LoggerMessage(EventId = 1029, Level = LogLevel.Error,
        Message = "Failed to revoke session {SessionId}")]
    internal static partial void RevokeSessionFailed(this ILogger logger, Exception exception, int sessionId);

    [LoggerMessage(EventId = 1030, Level = LogLevel.Error, Message = "Register failed: {Errors}")]
    internal static partial void RegisterFailed(this ILogger logger, object? errors);

    [LoggerMessage(EventId = 1031, Level = LogLevel.Error, Message = "Failed to load cards")]
    internal static partial void FailedToLoadCards(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1032, Level = LogLevel.Error, Message = "Failed to freeze card {CardId}")]
    internal static partial void FailedToFreezeCard(this ILogger logger, Exception exception, int cardId);

    [LoggerMessage(EventId = 1033, Level = LogLevel.Error, Message = "Failed to unfreeze card {CardId}")]
    internal static partial void FailedToUnfreezeCard(this ILogger logger, Exception exception, int cardId);

    [LoggerMessage(EventId = 1034, Level = LogLevel.Error, Message = "Failed to cancel card {CardId}")]
    internal static partial void FailedToCancelCard(this ILogger logger, Exception exception, int cardId);

    [LoggerMessage(EventId = 1035, Level = LogLevel.Error, Message = "Failed to issue card")]
    internal static partial void FailedToIssueCard(this ILogger logger, Exception exception);
}
