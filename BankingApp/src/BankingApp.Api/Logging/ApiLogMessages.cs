namespace BankingApp.Api.Logging;

using Microsoft.Extensions.Logging;

internal static partial class ApiLogMessages
{
    [LoggerMessage(EventId = 3003, Level = LogLevel.Warning, Message = "Token validation failed [{Code}]: {Description}")]
    internal static partial void TokenValidationFailed(this ILogger logger, string code, string description);

    [LoggerMessage(EventId = 3004, Level = LogLevel.Warning, Message = "Session lookup failed [{Code}]: {Description}")]
    internal static partial void SessionLookupFailed(this ILogger logger, string code, string description);
}
