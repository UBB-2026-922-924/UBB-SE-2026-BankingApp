namespace BankingApp.Domain.Aggregates.AccountAggregate;

using Entities;
using Events;
using Common.Errors;
using Common.Primitives;
using Enums;
using ErrorOr;
using ValueObjects;
using Currency = NodaMoney.Currency;
using Money = NodaMoney.Money;

public sealed class Account : AggregateRoot<int>
{
    private readonly List<Card> _cards = [];
    private readonly List<Transaction> _transactions = [];

    private Account()
    {
    }

    public int UserId { get; private set; }

    public string? AccountName { get; private set; }

    public Iban Iban { get; private set; } = default!;

    public Money Balance { get; private set; } = default!;

    public AccountType AccountType { get; private set; }

    public AccountStatus Status { get; private set; } = AccountStatus.Active;

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public Currency Currency => Balance.Currency;

    public static Account Open(int userId, Iban iban, Currency currency, AccountType accountType, string? accountName, DateTime createdAt)
    {
        return new Account
        {
            UserId = userId,
            Iban = iban,
            Balance = new Money(0m, currency),
            AccountType = accountType,
            AccountName = accountName,
            CreatedAt = createdAt
        };
    }

    public bool IsActive() => Status == AccountStatus.Active;

    public bool HasSufficientFunds(Money amount) => amount.Amount >= 0 && Balance >= amount;

    public bool UsesCurrency(Currency currency) => Balance.Currency == currency;

    public void Rename(string? accountName)
    {
        AccountName = accountName;
    }

    public ErrorOr<Money> Debit(Money amount, DateTime occurredOnUtc)
    {
        ErrorOr<Success> validation = ValidateMoneyOperation(amount);
        if (validation.IsError)
        {
            return validation.FirstError;
        }

        if (Balance < amount)
        {
            return AccountErrors.InsufficientFunds;
        }

        ChangeBalance(Balance - amount, occurredOnUtc);
        return Balance;
    }

    public ErrorOr<Money> Credit(Money amount, DateTime occurredOnUtc)
    {
        ErrorOr<Success> validation = ValidateMoneyOperation(amount);
        if (validation.IsError)
        {
            return validation.FirstError;
        }

        ChangeBalance(Balance + amount, occurredOnUtc);
        return Balance;
    }

    public void ChangeBalance(Money newBalance, DateTime occurredOnUtc)
    {
        Money oldBalance = Balance;
        Balance = newBalance;
        Raise(new BalanceUpdatedEvent(Id, oldBalance.Amount, newBalance.Amount, occurredOnUtc));
    }

    public Card IssueCard(
        string cardNumber,
        string cardholderName,
        DateTime expiryDate,
        string cvv,
        CardType cardType,
        string? cardBrand,
        DateTime createdAt)
    {
        var card = Card.Create(Id, UserId, cardNumber, cardholderName, expiryDate, cvv, cardType, cardBrand, createdAt);
        _cards.Add(card);
        return card;
    }

    public Transaction RecordTransaction(
        string transactionRef,
        string type,
        TransactionDirection direction,
        Money amount,
        Money balanceAfter,
        TransactionStatus status,
        DateTime createdAt)
    {
        var transaction = Transaction.Create(
            Id,
            transactionRef,
            type,
            direction,
            amount,
            balanceAfter,
            status,
            createdAt);

        _transactions.Add(transaction);
        Raise(new TransactionRecordedEvent(Id, transactionRef, createdAt));
        return transaction;
    }

    private ErrorOr<Success> ValidateMoneyOperation(Money amount)
    {
        if (amount.Amount < 0)
        {
            return AccountErrors.NegativeAmount;
        }

        if (!UsesCurrency(amount.Currency))
        {
            return AccountErrors.CurrencyMismatch;
        }

        return Result.Success;
    }
}
