namespace BankingApp.Domain.Aggregates.ChatAggregate.Entities;

using BankingApp.Domain.Common.Primitives;

public sealed class ChatAttachment : Entity<int>
{
    private ChatAttachment()
    {
    }

    private ChatAttachment(string fileName, string contentType, long fileSizeBytes, string storagePath)
    {
        FileName = fileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        StoragePath = storagePath;
    }

    public int ChatMessageId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;

    public static ChatAttachment Create(string fileName, string contentType, long fileSizeBytes, string storagePath)
    {
        return new ChatAttachment(fileName, contentType, fileSizeBytes, storagePath);
    }
}