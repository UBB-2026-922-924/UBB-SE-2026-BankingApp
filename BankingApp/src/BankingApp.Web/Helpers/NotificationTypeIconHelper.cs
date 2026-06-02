namespace BankingApp.Web.Helpers;

using Domain.Enums;

/// <summary>
///     Provides Bootstrap icon class mappings for <see cref="NotificationType"/> values.
/// </summary>
public static class NotificationTypeIconHelper
{
    /// <summary>Returns the Bootstrap icon CSS class for the given <paramref name="category"/>.</summary>
    public static string GetIconClass(NotificationType category) => category switch
    {
        NotificationType.Payment            => "bi-credit-card",
        NotificationType.InboundTransfer    => "bi-arrow-down-circle",
        NotificationType.OutboundTransfer   => "bi-arrow-up-circle",
        NotificationType.LowBalance         => "bi-exclamation-triangle",
        NotificationType.DuePayment         => "bi-calendar-check",
        NotificationType.SuspiciousActivity => "bi-shield-exclamation",
        _                                    => "bi-bell"
    };
}
