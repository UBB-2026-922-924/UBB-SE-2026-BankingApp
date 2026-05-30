namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

public static class ChatErrors
{
    public static Error SessionNotFound => Error.NotFound("Chat.SessionNotFound", "The chat session was not found.");
    public static Error MessageNotFound => Error.NotFound("Chat.MessageNotFound", "The chat message was not found.");
    public static Error SessionClosed => Error.Conflict("Chat.SessionClosed", "The chat session is closed.");
    public static Error EmptyMessage => Error.Validation("Chat.EmptyMessage", "A chat message cannot be empty.");
}