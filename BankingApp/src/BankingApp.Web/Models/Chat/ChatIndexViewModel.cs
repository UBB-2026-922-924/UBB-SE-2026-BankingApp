using BankingApp.Domain.Aggregates.ChatAggregate;

namespace BankingApp.Web.Models.Chat
{
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
}
