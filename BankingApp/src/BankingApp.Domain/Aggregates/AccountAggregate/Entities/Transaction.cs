namespace BankingApp.Domain.Aggregates.AccountAggregate.Entities;

using Common.Primitives;
using Enums;
using Money = NodaMoney.Money;

public sealed class Transaction : Entity<int>
{
    private Transaction()
    {
    }

    public int AccountId { get; private set; }

    public int? CardId { get; private set; }

    public string TransactionRef { get; private set; } = string.Empty;

    public string Type { get; private set; } = string.Empty;

    public TransactionDirection Direction { get; private set; }

    public Money Amount { get; private set; } = default!;

    public Money BalanceAfter { get; private set; } = default!;

    public string? CounterpartyName { get; private set; }

    public string? CounterpartyIban { get; private set; }

    public string? MerchantName { get; private set; }

    public int? CategoryId { get; private set; }

    public string? Description { get; private set; }

    public Money? Fee { get; private set; }

    public decimal? ExchangeRate { get; private set; }

    public TransactionStatus Status { get; private set; }

    public string? RelatedEntityType { get; private set; }

    public int? RelatedEntityId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Transaction Create(
        int accountId,
        string transactionRef,
        string type,
        TransactionDirection direction,
        Money amount,
        Money balanceAfter,
        TransactionStatus status,
        DateTime createdAt)
    {
        return new Transaction
        {
            AccountId = accountId,
            TransactionRef = transactionRef,
            Type = type,
            Direction = direction,
            Amount = amount,
            BalanceAfter = balanceAfter,
            Status = status,
            CreatedAt = createdAt
        };
    }

    public void Categorize(int categoryId)
    {
        CategoryId = categoryId;
    }

    public void LinkToRelatedEntity(string relatedEntityType, int relatedEntityId)
    {
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
    }
}
