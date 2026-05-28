namespace BankingApp.Infrastructure.Common.Logging;

using System;
using Microsoft.Extensions.Logging;

internal static partial class InfrastructureLogMessages
{
    [LoggerMessage(EventId = 4000, Level = LogLevel.Error, Message = "Failed to send email to {ToEmail} with subject '{Subject}'.")]
    internal static partial void EmailSendFailed(this ILogger logger, Exception exception, string toEmail, string subject);
}
