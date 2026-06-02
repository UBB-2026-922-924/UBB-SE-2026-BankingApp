namespace BankingApp.Domain.Aggregates.SavingsAggregate.Entities;

using System;
using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;

public sealed class SavingsTransaction : Entity<int>
{
    private SavingsTransaction()
    {
    }

    private SavingsTransaction(int savingsAccountId, decimal amount, SavingsTransactionType type, string? source, int accountId, decimal balanceAfter, DateTime createdAt, string? description)
    {
        SavingsAccountId = savingsAccountId;
        Amount = amount;
        Type = type;
        Source = source;
        AccountId = accountId;
        BalanceAfter = balanceAfter;
        CreatedAt = createdAt;
        Description = description;
    }

    public int SavingsAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public SavingsTransactionType Type { get; private set; }
    public string? Source { get; private set; }
    public int AccountId { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? Description { get; private set; }

    public static SavingsTransaction Create(int savingsAccountId, decimal amount, SavingsTransactionType type, string? source, int accountId, decimal balanceAfter, DateTime createdAt, string? description = null)
        => new(savingsAccountId, amount, type, source, accountId, balanceAfter, createdAt, description);
}
