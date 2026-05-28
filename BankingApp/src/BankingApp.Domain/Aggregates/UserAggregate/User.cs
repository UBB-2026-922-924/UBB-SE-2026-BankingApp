namespace BankingApp.Domain.Aggregates.UserAggregate;

using Entities;
using Common.Primitives;
using Enums;
using ValueObjects;

public sealed class User : AggregateRoot<int>
{
    private readonly List<Notification> _notifications = [];
    private readonly List<NotificationPreference> _notificationPreferences = [];

    private User()
    {
    }

    public Email Email { get; private set; } = null!;

    public string FullName { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public DateTime? DateOfBirth { get; private set; }

    public string? Address { get; private set; }

    public string? Nationality { get; private set; }

    public string PreferredLanguage { get; private set; } = "en";

    public IReadOnlyCollection<NotificationPreference> NotificationPreferences => _notificationPreferences.AsReadOnly();

    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static User Register(Email email, string fullName, DateTime createdAt)
    {
        return new User
        {
            Email = email,
            FullName = fullName,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    public void UpdateProfile(
        string fullName,
        string? phoneNumber,
        DateTime? dateOfBirth,
        string? address,
        string? nationality,
        string preferredLanguage,
        DateTime updatedAt)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        Address = address;
        Nationality = nationality;
        PreferredLanguage = preferredLanguage;
        UpdatedAt = updatedAt;
    }

    public void SetNotificationPreference(NotificationType category, bool pushEnabled, bool emailEnabled, bool smsEnabled, decimal? minAmountThreshold)
    {
        NotificationPreference? existing = _notificationPreferences.SingleOrDefault(preference => preference.Category == category);
        if (existing is null)
        {
            _notificationPreferences.Add(NotificationPreference.Create(Id, category, pushEnabled, emailEnabled, smsEnabled, minAmountThreshold));
            return;
        }

        existing.Update(pushEnabled, emailEnabled, smsEnabled, minAmountThreshold);
    }

    public Notification AddNotification(
        string title,
        string message,
        string type,
        string channel,
        string? relatedEntityType,
        int? relatedEntityId,
        DateTime createdAt)
    {
        var notification = Notification.Create(
            Id,
            title,
            message,
            type,
            channel,
            relatedEntityType,
            relatedEntityId,
            createdAt);

        _notifications.Add(notification);
        return notification;
    }
}
