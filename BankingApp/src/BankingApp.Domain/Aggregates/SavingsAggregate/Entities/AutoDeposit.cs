namespace BankingApp.Domain.Aggregates.SavingsAggregate.Entities;

using System;
using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;

/// <summary>Represents an automatic recurring transfer into a savings account.</summary>
public sealed class AutoDeposit : Entity<int>
{
    private AutoDeposit()
    {
    }

    private AutoDeposit(int savingsAccountId, decimal amount, DepositFrequency frequency, DateTime nextRunDate, bool isActive, int? sourceAccountId, int? dayOfMonth, int? dayOfWeek)
    {
        SavingsAccountId = savingsAccountId;
        Amount = amount;
        Frequency = frequency;
        NextRunDate = nextRunDate;
        IsActive = isActive;
        SourceAccountId = sourceAccountId;
        DayOfMonth = dayOfMonth;
        DayOfWeek = dayOfWeek;
    }

    public int SavingsAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public DepositFrequency Frequency { get; private set; }
    public DateTime NextRunDate { get; private set; }
    public bool IsActive { get; private set; }
    public int? SourceAccountId { get; private set; }
    public int? DayOfMonth { get; private set; }
    public int? DayOfWeek { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static AutoDeposit Create(int savingsAccountId, decimal amount, DepositFrequency frequency, DateTime nextRunDate, bool isActive, int? sourceAccountId = null, int? dayOfMonth = null, int? dayOfWeek = null)
        => new(savingsAccountId, amount, frequency, nextRunDate, isActive, sourceAccountId, dayOfMonth, dayOfWeek);

    /// <summary>Rebuilds an AutoDeposit from persisted storage state (infrastructure use only).</summary>
    public static AutoDeposit Reconstitute(int id, int savingsAccountId, decimal amount, DepositFrequency frequency, DateTime nextRunDate, bool isActive, int? sourceAccountId, int? dayOfMonth, int? dayOfWeek, DateTime? updatedAt)
        => new()
        {
            Id = id,
            SavingsAccountId = savingsAccountId,
            Amount = amount,
            Frequency = frequency,
            NextRunDate = nextRunDate,
            IsActive = isActive,
            SourceAccountId = sourceAccountId,
            DayOfMonth = dayOfMonth,
            DayOfWeek = dayOfWeek,
            UpdatedAt = updatedAt,
        };
}
