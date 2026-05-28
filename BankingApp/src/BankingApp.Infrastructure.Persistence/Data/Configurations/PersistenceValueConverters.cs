namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using System.Globalization;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Currency = NodaMoney.Currency;
using Money = NodaMoney.Money;

internal static class PersistenceValueConverters
{
    internal static readonly ValueConverter<Email, string> EmailConverter = new(
        email => email.Value,
        value => Email.Create(value).Value);

    internal static readonly ValueConverter<Iban, string> IbanConverter = new(
        iban => iban.Value,
        value => Iban.Create(value).Value);

    internal static readonly ValueConverter<HashedPassword, string> HashedPasswordConverter = new(
        hash => hash.Value,
        value => HashedPassword.Wrap(value));

    internal static readonly ValueConverter<HashedPassword?, string?> NullableHashedPasswordConverter = new(
        hash => hash == null ? null : hash.Value,
        value => value == null ? null : HashedPassword.Wrap(value));

    internal static readonly ValueConverter<Money, string> MoneyConverter = new(
        money => SerializeMoney(money),
        value => DeserializeMoney(value));

    internal static readonly ValueConverter<Money?, string?> NullableMoneyConverter = new(
        money => money == null ? null : SerializeMoney(money.Value),
        value => value == null ? null : DeserializeMoney(value));

    internal static readonly ValueConverter<Currency, string> CurrencyConverter = new(
        currency => currency.Code,
        value => Currency.FromCode(value));

    internal static readonly ValueComparer<Money> MoneyComparer = new(
        (left, right) => left.Amount == right.Amount && left.Currency == right.Currency,
        money => HashCode.Combine(money.Amount, money.Currency.Code),
        money => new Money(money.Amount, money.Currency));

    internal static readonly ValueComparer<Money?> NullableMoneyComparer = new(
        (left, right) =>
            left.HasValue == right.HasValue &&
            (!left.HasValue || left.Value.Amount == right!.Value.Amount && left.Value.Currency == right.Value.Currency),
        money => money.HasValue ? HashCode.Combine(money.Value.Amount, money.Value.Currency.Code) : 0,
        money => money.HasValue ? new Money?(new Money(money.Value.Amount, money.Value.Currency)) : null);

    private static string SerializeMoney(Money money) =>
        $"{money.Amount.ToString(CultureInfo.InvariantCulture)}|{money.Currency.Code}";

    private static Money DeserializeMoney(string value)
    {
        string[] parts = value.Split('|', 2, StringSplitOptions.TrimEntries);
        decimal amount = decimal.Parse(parts[0], CultureInfo.InvariantCulture);
        var currency = Currency.FromCode(parts[1]);
        return new Money(amount, currency);
    }
}
