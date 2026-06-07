namespace BankingApp.Infrastructure.Core.Caching;

using Application;
using Application.Features.Forex.Services;
using Microsoft.Extensions.Caching.Memory;
using Currency = NodaMoney.Currency;

public sealed class MemoryLockedRateCache(IMemoryCache memoryCache) : ILockedRateCache
{
    private static readonly TimeSpan _lockTtl = TimeSpan.FromSeconds(300);

    public void Store(int userId, Currency baseCurrency, Currency quoteCurrency, decimal rate, DateTime lockedAt)
    {
        memoryCache.Set(Key(userId), new LockedRate(baseCurrency, quoteCurrency, rate, lockedAt), _lockTtl);
    }

    public LockedRate? TryGet(int userId)
    {
        return memoryCache.TryGetValue(Key(userId), out LockedRate? lockedRate) ? lockedRate : null;
    }

    public void Remove(int userId)
    {
        memoryCache.Remove(Key(userId));
    }

    private static string Key(int userId) => $"forex_lock:{userId}";
}
