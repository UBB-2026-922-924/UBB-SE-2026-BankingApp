namespace BankingApp.Web.Models.Chat;

using Contracts.Features.Chat.Dtos;

public class ChatDetailsViewModel
{
    public required ChatSessionDto Session { get; set; }

    public List<ChatMessageDto> Messages { get; set; } = [];

    public string? NewMessageContent { get; set; }

    public List<string> PresetQuestions { get; set; } =
    [
        "How do I reset my password?",
        "Why was my card declined?",
        "How long does a transfer take?",
        "How do I upload documents for support?",
        "I found a technical problem in the app."
    ];
}
