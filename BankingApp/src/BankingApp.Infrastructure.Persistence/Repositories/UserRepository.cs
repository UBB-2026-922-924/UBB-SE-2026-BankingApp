namespace BankingApp.Infrastructure.Persistence.Repositories;

using Data;
using Domain.Aggregates.UserAggregate;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(user => user.NotificationPreferences)
            .Include(user => user.Notifications)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(user => user.NotificationPreferences)
            .Include(user => user.Notifications)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        return Task.CompletedTask;
    }
}
