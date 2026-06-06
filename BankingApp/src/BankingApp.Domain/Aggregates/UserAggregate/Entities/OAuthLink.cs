namespace BankingApp.Domain.Aggregates.UserAggregate.Entities;

using Common.Primitives;

public sealed class OAuthLink : Entity<int>
{
    private OAuthLink()
    {
    }

    public int UserId { get; private set; }

    public string Provider { get; private set; } = string.Empty;

    public string ProviderUserId { get; private set; } = string.Empty;

    public string? ProviderEmail { get; private set; }

    public DateTime LinkedAt { get; private set; }

    public static OAuthLink Create(
        int userId,
        string provider,
        string providerUserId,
        string? providerEmail,
        DateTime linkedAt)
    {
        return new OAuthLink
        {
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            ProviderEmail = providerEmail,
            LinkedAt = linkedAt
        };
    }
}
