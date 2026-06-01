namespace BankingApp.Contracts.Features.Chat.Dtos;

public class SaveChatFeedbackRequest
{
    public int Rating { get; set; }
    public string? Feedback { get; set; }
}