namespace BankingApp.Infrastructure.Http.Features.Chat.Services;

using Contracts.Features.Chat.Dtos;
using Contracts.Http;
using Shared.Http;

public class ChatRepoProxy(ApiService apiService) : IChatRepoProxy
{
    public Task<List<ChatSessionDto>> GetSessionsAsync()
    {
        return apiService.GetAsync<List<ChatSessionDto>>(ApiEndpoints.Chat.Base);
    }

    public Task<ChatSessionDto> GetSessionAsync(int sessionId)
    {
        return apiService.GetAsync<ChatSessionDto>(ApiEndpoints.Chat.SessionByIdFull(sessionId));
    }

    public Task<ChatSessionDto> CreateSessionAsync(string subject)
    {
        return apiService.PostAsync<CreateChatSessionRequest, ChatSessionDto>(
            ApiEndpoints.Chat.Base,
            new CreateChatSessionRequest { Subject = subject });
    }

    public Task<ChatMessageDto> CreateMessageAsync(int sessionId, string content)
    {
        return apiService.PostAsync<CreateChatMessageRequest, ChatMessageDto>(
            ApiEndpoints.Chat.SessionMessagesFull(sessionId),
            new CreateChatMessageRequest { Content = content });
    }

    public Task CloseSessionAsync(int sessionId)
    {
        return apiService.PutAsync<object>(ApiEndpoints.Chat.SessionByIdFull(sessionId), new { });
    }

    public Task SaveFeedbackAsync(int sessionId, int rating, string? feedback)
    {
        return apiService.PostAsync(
            ApiEndpoints.Chat.SessionFeedbackFull(sessionId),
            new SaveChatFeedbackRequest { Rating = rating, Feedback = feedback });
    }
}
