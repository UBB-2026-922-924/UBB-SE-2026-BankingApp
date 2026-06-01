namespace BankingApp.Contracts.Features.Chat.Dtos;

public class ChatMessageDto
{
    public int Id { get; set; }
    public int ChatSessionId { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}