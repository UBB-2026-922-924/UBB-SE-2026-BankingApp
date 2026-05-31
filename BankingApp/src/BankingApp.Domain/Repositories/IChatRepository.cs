namespace BankingApp.Domain.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Domain.Aggregates.ChatAggregate;

/// <summary>
///     Persistence boundary for chat support sessions and their messages.
/// </summary>
public interface IChatRepository
{
    public Task<IReadOnlyCollection<ChatSession>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ChatSession?> GetByIdAsync(int sessionId, CancellationToken cancellationToken = default);
    public Task AddAsync(ChatSession session, CancellationToken cancellationToken = default);
    public Task UpdateAsync(ChatSession session, CancellationToken cancellationToken = default);
    public Task DeleteAsync(ChatSession session, CancellationToken cancellationToken = default);
}