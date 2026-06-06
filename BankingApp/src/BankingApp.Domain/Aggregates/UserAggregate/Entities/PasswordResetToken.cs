namespace BankingApp.Domain.Aggregates.UserAggregate.Entities;

using Common.Primitives;

public sealed class PasswordResetToken : Entity<int>
{
    private PasswordResetToken()
    {
    }

    public int UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public DateTime? UsedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static PasswordResetToken Create(
        int userId,
        string tokenHash,
        DateTime expiresAt,
        DateTime? usedAt,
        DateTime createdAt)
    {
        return new PasswordResetToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            UsedAt = usedAt,
            CreatedAt = createdAt
        };
    }
}
