namespace BankingApp.Infrastructure.Http.Shared.Logging;

using System;
using Microsoft.Extensions.Logging;

internal static partial class InfrastructureHttpLogMessages
{
    [LoggerMessage(EventId = 5000, Level = LogLevel.Warning, Message = "API base URL is missing from configuration.")]
    internal static partial void ApiBaseUrlMissing(this ILogger logger);

    [LoggerMessage(EventId = 5001, Level = LogLevel.Debug, Message = "API auth token set.")]
    internal static partial void ApiTokenSet(this ILogger logger);

    [LoggerMessage(EventId = 5002, Level = LogLevel.Debug, Message = "API auth token cleared.")]
    internal static partial void ApiTokenCleared(this ILogger logger);

    [LoggerMessage(EventId = 5003, Level = LogLevel.Warning, Message = "{Operation} request failed with HTTP {StatusCode}: {Message}")]
    internal static partial void HttpRequestFailed(this ILogger logger, string operation, int statusCode, string message);

    [LoggerMessage(EventId = 5004, Level = LogLevel.Error, Message = "{Operation} request failed due to a transport error.")]
    internal static partial void HttpRequestTransportFailed(this ILogger logger, Exception exception, string operation);

    [LoggerMessage(EventId = 5005, Level = LogLevel.Warning, Message = "{Operation} request was cancelled.")]
    internal static partial void HttpRequestCancelled(this ILogger logger, string operation);

    [LoggerMessage(EventId = 5006, Level = LogLevel.Warning, Message = "{Operation} returned an empty response.")]
    internal static partial void HttpEmptyResponse(this ILogger logger, string operation);
}
