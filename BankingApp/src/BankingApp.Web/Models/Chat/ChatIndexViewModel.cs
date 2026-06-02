namespace BankingApp.Web.Models.Chat;

using Contracts.Features.Chat.Dtos;

public class ChatIndexViewModel
{
    public List<ChatSessionDto> Sessions { get; set; } = [];

    public List<string> Categories { get; set; } =
    [
        "Account", "Cards", "Transfers", "Loans", "Technical Issues", "Other"
    ];

    public string? SelectedCategory { get; set; }

    public string? ErrorMessage { get; set; }
}
