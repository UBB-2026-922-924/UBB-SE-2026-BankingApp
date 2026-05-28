namespace BankingApp.Domain.Aggregates.IdentityAggregate.Entities;

using Common.Primitives;

public sealed class Session : Entity<int>
{
    private Session()
    {
    }

    public int IdentityAccountId { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public string? DeviceInfo { get; private set; }

    public string? Browser { get; private set; }

    public string? IpAddress { get; private set; }

    public DateTime? LastActiveAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public bool IsRevoked { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Session Create(
        int identityAccountId,
        string token,
        DateTime expiresAt,
        DateTime createdAt,
        string? deviceInfo,
        string? browser,
        string? ipAddress)
    {
        return new Session
        {
            IdentityAccountId = identityAccountId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = createdAt,
            DeviceInfo = deviceInfo,
            Browser = browser,
            IpAddress = ipAddress
        };
    }

    public void RefreshActivity(DateTime lastActiveAt)
    {
        LastActiveAt = lastActiveAt;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}
