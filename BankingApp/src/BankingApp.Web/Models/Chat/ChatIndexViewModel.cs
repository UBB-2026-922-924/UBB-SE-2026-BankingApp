namespace BankingApp.Web.Models.Chat;

using BankingApp.Domain.Aggregates.ChatAggregate;

public class ChatIndexViewModel
{

    public List<ChatSession> Sessions { get; set; } = new();
    public List<string> Categories { get; set; } = new()
    {
        "Account", "Cards", "Transfers", "Loans", "Technical Issues", "Other"
    };

    public string? SelectedCategory { get; set; }
    public string? errorMessage { get; set; }
}
