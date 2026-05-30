namespace BankingApp.Domain.Aggregates.ChatAggregate.Entities;

using System.Collections.Generic;
using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;
public sealed class ChatMessage : Entity<int>
{
    private readonly List<ChatAttachment> _attachments = [];

    private ChatMessage()
    {
    }

    private ChatMessage(ChatMessageSender sender, string content, DateTime sentAt)
    {
        Sender = sender;
        Content = content;
        SentAt = sentAt;
    }

    public int ChatSessionId { get; private set; }
    public ChatMessageSender Sender { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }
    public IReadOnlyCollection<ChatAttachment> Attachments => _attachments.AsReadOnly();

    public static ChatMessage Create(ChatMessageSender sender, string content, DateTime sentAt)
    {
        return new ChatMessage(sender, content, sentAt);
    }

    public ChatAttachment AddAttachment(string fileName, string contentType, long fileSizeBytes, string storagePath)
    {
        var attachment = ChatAttachment.Create(fileName, contentType, fileSizeBytes, storagePath);
        _attachments.Add(attachment);
        return attachment;
    }
}