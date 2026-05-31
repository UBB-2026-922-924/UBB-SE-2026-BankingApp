namespace BankingApp.Application.Features.Chat.Services;

using Contracts.Features.Chat.Dtos;
using ErrorOr;

public interface IChatService
{
    public Task<ErrorOr<List<ChatSessionDto>>> GetSessionsAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<ChatSessionDto>> GetSessionAsync(int userId, int sessionId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<ChatSessionDto>> CreateSessionAsync(int userId, string subject, CancellationToken cancellationToken = default);
    public Task<ErrorOr<ChatMessageDto>> PostMessageAsync(int userId, int sessionId, string content, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> CloseSessionAsync(int userId, int sessionId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> SaveFeedbackAsync(int userId, int sessionId, int rating, string? feedback, CancellationToken cancellationToken = default);
}