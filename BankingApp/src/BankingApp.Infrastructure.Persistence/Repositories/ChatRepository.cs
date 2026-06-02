namespace BankingApp.Infrastructure.Persistence.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Domain.Aggregates.ChatAggregate;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public sealed class ChatRepository(AppDbContext dbContext) : IChatRepository
{
    public async Task<IReadOnlyCollection<ChatSession>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ChatSessions
            .Include(s => s.Messages)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatSession?> GetByIdAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task AddAsync(ChatSession session, CancellationToken cancellationToken = default)
    {
        await dbContext.ChatSessions.AddAsync(session, cancellationToken);
    }

    public Task UpdateAsync(ChatSession session, CancellationToken cancellationToken = default)
    {
        dbContext.ChatSessions.Update(session);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ChatSession session, CancellationToken cancellationToken = default)
    {
        dbContext.ChatSessions.Remove(session);
        return Task.CompletedTask;
    }
}
