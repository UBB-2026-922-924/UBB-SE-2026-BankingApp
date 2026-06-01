namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Chat
    {
        public const string Base = $"{ApiBase}/chat";
        public const string ById = "{id}";
        public const string Messages = "{id}/messages";
        public const string Feedback = "{id}/feedback";
    }
}