namespace BankingApp.Application;

using Currency = NodaMoney.Currency;

public sealed record LockedRate(
    Currency BaseCurrency,
    Currency QuoteCurrency,
    decimal Rate,
    DateTime LockedAt);
