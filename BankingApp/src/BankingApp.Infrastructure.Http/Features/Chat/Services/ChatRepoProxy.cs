namespace BankingApp.Infrastructure.Http.Features.Chat.Services;

using BankingApp.Contracts.Features.Chat.Dtos;
using BankingApp.Contracts.Http;
using Domain.Aggregates.ChatAggregate;
using Domain.Aggregates.ChatAggregate.Entities;
using Shared.Http;

public class ChatRepoProxy(ApiService apiService) : IChatRepoProxy
{
    public Task<List<ChatSession>?> GetSessionsAsync()
    {
        return apiService.GetAsync<List<ChatSession>>(ApiEndpoints.Chat.SessionsFull);
    }

    public Task<ChatSession?> GetSessionAsync(int sessionId)
    {
        return apiService.GetAsync<ChatSession>(ApiEndpoints.Chat.SessionByIdFull(sessionId));
    }

    public Task<CreateChatSessionResponse?> CreateSessionAsync(string issueCategory)
    {
        return apiService.PostAsync<object, CreateChatSessionResponse>(
            ApiEndpoints.Chat.SessionsFull,
            new { issueCategory });
    }

    public Task<List<ChatMessage>?> GetMessagesAsync(int sessionId)
    {
        return apiService.GetAsync<List<ChatMessage>>(ApiEndpoints.Chat.SessionMessagesFull(sessionId));
    }

    public Task<CreateChatMessageResponse?> CreateMessageAsync(int sessionId, string senderType, string content)
    {
        return apiService.PostAsync<object, CreateChatMessageResponse>(
            ApiEndpoints.Chat.SessionMessagesFull(sessionId),
            new { senderType, content });
    }

    public Task<CreateChatAttachmentResponse?> CreateAttachmentAsync(int messageId, CreateChatAttachmentRequest request)
    {
        return apiService.PostAsync<CreateChatAttachmentRequest, CreateChatAttachmentResponse>(
            ApiEndpoints.Chat.MessageAttachmentsFull(messageId),
            request);
    }

    public Task<OperationResponse?> UpdateSessionStatusAsync(int sessionId, string status)
    {
        return apiService.PutAsync<object, OperationResponse>(
            ApiEndpoints.Chat.SessionStatusFull(sessionId),
            new { status });
    }

    public Task<OperationResponse?> SaveFeedbackAsync(int sessionId, int rating, string feedback)
    {
        return apiService.PostAsync<object, OperationResponse>(
            ApiEndpoints.Chat.SessionFeedbackFull(sessionId),
            new { rating, feedback });
    }

    public Task<OperationResponse?> EmailTranscriptAsync(int sessionId, string email)
    {
        return apiService.PostAsync<object, OperationResponse>(
            ApiEndpoints.Chat.SessionTranscriptEmailFull(sessionId),
            new { email });
    }
}
