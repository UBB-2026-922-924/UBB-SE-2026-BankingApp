namespace BankingApp.Application.Features.Chat.Services;

using Domain.Enums;
using Contracts.Features.Chat.Dtos;
using Domain.Aggregates.ChatAggregate;
using Domain.Aggregates.ChatAggregate.Entities;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;

public sealed class ChatService(
    IChatRepository chatRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock)
    : IChatService
{
    public async Task<ErrorOr<List<ChatSessionDto>>> GetSessionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<ChatSession> sessions = await chatRepository.ListByUserIdAsync(userId, cancellationToken);
        return sessions.Select(MapSession).ToList();
    }

    public async Task<ErrorOr<ChatSessionDto>> GetSessionAsync(int userId, int sessionId, CancellationToken cancellationToken = default)
    {
        ChatSession? session = await chatRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null || session.UserId != userId)
        {
            return ChatErrors.SessionNotFound;
        }

        return MapSession(session);
    }

    public async Task<ErrorOr<ChatSessionDto>> CreateSessionAsync(int userId, string subject, CancellationToken cancellationToken = default)
    {
        var session = ChatSession.Start(userId, subject.Trim(), clock.UtcNow);
        await chatRepository.AddAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapSession(session);
    }

    public async Task<ErrorOr<ChatMessageDto>> PostMessageAsync(int userId, int sessionId, string content, CancellationToken cancellationToken = default)
    {
        ChatSession? session = await chatRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null || session.UserId != userId)
        {
            return ChatErrors.SessionNotFound;
        }

        ErrorOr<ChatMessage> messageResult = session.PostMessage(ChatMessageSender.Customer, content, clock.UtcNow);
        if (messageResult.IsError)
        {
            return messageResult.FirstError;
        }

        await chatRepository.UpdateAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapMessage(messageResult.Value);
    }

    public async Task<ErrorOr<Success>> CloseSessionAsync(int userId, int sessionId, CancellationToken cancellationToken = default)
    {
        ChatSession? session = await chatRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null || session.UserId != userId)
        {
            return ChatErrors.SessionNotFound;
        }

        session.Close(clock.UtcNow);
        await chatRepository.UpdateAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> SaveFeedbackAsync(int userId, int sessionId, int rating, string? feedback, CancellationToken cancellationToken = default)
    {
        ChatSession? session = await chatRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null || session.UserId != userId)
        {
            return ChatErrors.SessionNotFound;
        }

        session.RecordFeedback(rating, feedback, clock.UtcNow);
        await chatRepository.UpdateAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }

    private static ChatSessionDto MapSession(ChatSession session)
    {
        return new ChatSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            Subject = session.Subject,
            Status = session.Status.ToString(),
            Rating = session.Rating,
            Feedback = session.Feedback,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            Messages = session.Messages.Select(MapMessage).ToList()
        };
    }

    private static ChatMessageDto MapMessage(ChatMessage message)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            ChatSessionId = message.ChatSessionId,
            Sender = message.Sender.ToString(),
            Content = message.Content,
            SentAt = message.SentAt
        };
    }
}