namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Chat
    {
        public const string Base = $"{ApiBase}/chat";
        public const string ById = "{id}";
        public const string Messages = "{id}/messages";
        public const string Feedback = "{id}/feedback";
        public const string Sessions = "sessions";

        public const string SessionsFull = $"{Base}/{Sessions}";

        public static string SessionByIdFull(int sessionId) => $"{SessionsFull}/{sessionId}";

        public static string SessionMessagesFull(int sessionId) => $"{SessionsFull}/{sessionId}/messages";

        public static string MessageAttachmentsFull(int messageId) => $"{Base}/messages/{messageId}/attachments";

        public static string SessionStatusFull(int sessionId) => $"{SessionsFull}/{sessionId}/status";

        public static string SessionFeedbackFull(int sessionId) => $"{SessionsFull}/{sessionId}/feedback";

        public static string SessionTranscriptEmailFull(int sessionId) => $"{SessionsFull}/{sessionId}/transcript/email";
    }
}
