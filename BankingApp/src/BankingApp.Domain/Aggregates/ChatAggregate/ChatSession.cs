namespace BankingApp.Domain.Aggregates.ChatAggregate;

using System.Collections.Generic;
using BankingApp.Domain.Aggregates.ChatAggregate.Entities;
using BankingApp.Domain.Common.Errors;
using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;
using ErrorOr;

public sealed class ChatSession : AggregateRoot<int>
{
    private readonly List<ChatMessage> _messages = [];

    // Required by EF Core.
    private ChatSession()
    {
    }

    private ChatSession(int userId, string subject, DateTime createdAt)
    {
        UserId = userId;
        Subject = subject;
        Status = ChatSessionStatus.Open;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public int UserId { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public ChatSessionStatus Status { get; private set; }
    public int? Rating { get; private set; }
    public string? Feedback { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();

    public static ChatSession Start(int userId, string subject, DateTime createdAt)
    {
        return new ChatSession(userId, subject, createdAt);
    }

    public ErrorOr<ChatMessage> PostMessage(ChatMessageSender sender, string content, DateTime sentAt)
    {
        if (Status == ChatSessionStatus.Closed)
        {
            return ChatErrors.SessionClosed;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return ChatErrors.EmptyMessage;
        }

        var message = ChatMessage.Create(sender, content.Trim(), sentAt);
        _messages.Add(message);
        UpdatedAt = sentAt;
        return message;
    }

    public void Close(DateTime closedAt)
    {
        Status = ChatSessionStatus.Closed;
        UpdatedAt = closedAt;
    }

    public void RecordFeedback(int rating, string? feedback, DateTime recordedAt)
    {
        Rating = rating;
        Feedback = feedback;
        UpdatedAt = recordedAt;
    }
}