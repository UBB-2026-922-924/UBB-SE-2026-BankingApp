namespace BankingApp.Domain.Aggregates.UserAggregate.Entities;

using Common.Primitives;

public sealed class Notification : Entity<int>
{
    private Notification()
    {
    }

    public int UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public string Type { get; private set; } = string.Empty;

    public string Channel { get; private set; } = string.Empty;

    public bool IsRead { get; private set; }

    public string? RelatedEntityType { get; private set; }

    public int? RelatedEntityId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Notification Create(
        int userId,
        string title,
        string message,
        string type,
        string channel,
        string? relatedEntityType,
        int? relatedEntityId,
        DateTime createdAt)
    {
        return new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Channel = channel,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            CreatedAt = createdAt
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
