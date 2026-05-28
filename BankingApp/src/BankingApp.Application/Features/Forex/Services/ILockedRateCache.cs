namespace BankingApp.Application.Features.Forex.Services;

using Currency = NodaMoney.Currency;

public interface ILockedRateCache
{
    public void Store(int userId, Currency baseCurrency, Currency quoteCurrency, decimal rate, DateTime lockedAt);
    public LockedRate? TryGet(int userId);
    public void Remove(int userId);
}
