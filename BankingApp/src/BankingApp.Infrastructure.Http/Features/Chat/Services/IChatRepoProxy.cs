namespace BankingApp.Infrastructure.Http.Features.Chat.Services;

using Contracts.Features.Chat.Dtos;

public interface IChatRepoProxy
{
    public Task<List<ChatSessionDto>> GetSessionsAsync();

    public Task<ChatSessionDto> GetSessionAsync(int sessionId);

    public Task<ChatSessionDto> CreateSessionAsync(string subject);

    public Task<ChatMessageDto> CreateMessageAsync(int sessionId, string content);

    public Task CloseSessionAsync(int sessionId);

    public Task SaveFeedbackAsync(int sessionId, int rating, string? feedback);
}
