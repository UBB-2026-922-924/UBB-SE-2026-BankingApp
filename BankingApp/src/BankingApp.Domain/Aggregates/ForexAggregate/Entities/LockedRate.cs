namespace BankingApp.Domain.Aggregates.ForexAggregate.Entities;

using Common.Primitives;
using Currency = NodaMoney.Currency;

public sealed class LockedRate : Entity<int>
{
    private const int LockDurationSeconds = 30;

    private LockedRate()
    {
    }

    public int UserId { get; private set; }

    public Currency BaseCurrency { get; private set; }

    public Currency QuoteCurrency { get; private set; }

    public decimal Rate { get; private set; }

    public DateTime LockedAt { get; private set; }

    public static LockedRate Create(int userId, Currency baseCurrency, Currency quoteCurrency, decimal rate, DateTime lockedAt)
    {
        return new LockedRate
        {
            UserId = userId,
            BaseCurrency = baseCurrency,
            QuoteCurrency = quoteCurrency,
            Rate = rate,
            LockedAt = lockedAt
        };
    }

    public bool IsExpired() => DateTime.UtcNow - LockedAt > TimeSpan.FromSeconds(LockDurationSeconds);
}
