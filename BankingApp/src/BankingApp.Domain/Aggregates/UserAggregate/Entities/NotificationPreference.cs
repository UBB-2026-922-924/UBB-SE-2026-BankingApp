namespace BankingApp.Domain.Aggregates.UserAggregate.Entities;

using Common.Primitives;
using Enums;

public sealed class NotificationPreference : Entity<int>
{
    private NotificationPreference()
    {
    }

    public int UserId { get; private set; }

    public NotificationType Category { get; private set; }

    public bool PushEnabled { get; private set; }

    public bool EmailEnabled { get; private set; }

    public bool SmsEnabled { get; private set; }

    public decimal? MinAmountThreshold { get; private set; }

    public static NotificationPreference Create(
        int userId,
        NotificationType category,
        bool pushEnabled,
        bool emailEnabled,
        bool smsEnabled,
        decimal? minAmountThreshold)
    {
        return new NotificationPreference
        {
            UserId = userId,
            Category = category,
            PushEnabled = pushEnabled,
            EmailEnabled = emailEnabled,
            SmsEnabled = smsEnabled,
            MinAmountThreshold = minAmountThreshold
        };
    }

    public void Update(bool pushEnabled, bool emailEnabled, bool smsEnabled, decimal? minAmountThreshold)
    {
        PushEnabled = pushEnabled;
        EmailEnabled = emailEnabled;
        SmsEnabled = smsEnabled;
        MinAmountThreshold = minAmountThreshold;
    }
}
