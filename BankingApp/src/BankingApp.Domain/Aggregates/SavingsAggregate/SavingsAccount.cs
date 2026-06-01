namespace BankingApp.Domain.Aggregates.SavingsAggregate;

using System;
using System.Collections.Generic;
using Common.Errors;
using Common.Primitives;
using Entities;
using Enums;
using ErrorOr;

/// <summary>
/// Manages and displays information about a user's savings account.
/// </summary>
public sealed class SavingsAccount : AggregateRoot<int>
{
    private const decimal MonthsInYear = 12m;
    private const decimal PercentageScale = 100m;

    private readonly List<AutoDeposit> _autoDeposits = [];
    private readonly List<SavingsTransaction> _transactions = [];

    private SavingsAccount()
    {
    }

    private SavingsAccount(
        int userId,
        SavingsType savingsType,
        decimal annualPercentageYield,
        string? accountName,
        int? fundingAccountId,
        decimal? targetAmount,
        DateTime? targetDate,
        DateTime? maturityDate,
        DateTime createdAt)
    {
        UserId = userId;
        SavingsType = savingsType;
        AnnualPercentageYield = annualPercentageYield;
        AccountName = accountName;
        FundingAccountId = fundingAccountId;
        TargetAmount = targetAmount;
        TargetDate = targetDate;
        MaturityDate = maturityDate;
        Balance = 0;
        AccruedInterest = 0;
        AccountStatus = "Active";
        CreatedAt = createdAt;
    }

    public int UserId { get; private set; }
    public SavingsType SavingsType { get; private set; }
    public decimal Balance { get; private set; }
    public decimal AccruedInterest { get; private set; }
    public decimal AnnualPercentageYield { get; private set; }
    public DateTime? MaturityDate { get; private set; }
    public string AccountStatus { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? AccountName { get; private set; }
    public int? FundingAccountId { get; private set; }
    public decimal? TargetAmount { get; private set; }
    public DateTime? TargetDate { get; private set; }

    public IReadOnlyCollection<AutoDeposit> AutoDeposits => _autoDeposits.AsReadOnly();
    public IReadOnlyCollection<SavingsTransaction> Transactions => _transactions.AsReadOnly();

    public decimal MonthlyInterestProjection => Balance * AnnualPercentageYield / MonthsInYear;

    public double ProgressPercent =>
        TargetAmount > 0
            ? (double)(Balance / TargetAmount.Value * PercentageScale)
            : 0d;

    public string FormattedBalance => $"${Balance:N2}";

    public bool IsGoalSavings => SavingsType == SavingsType.GoalSavings;

    public string DisplayStatus =>
        SavingsType == SavingsType.FixedDeposit &&
        MaturityDate.HasValue &&
        MaturityDate.Value <= DateTime.UtcNow
            ? "Matured"
            : AccountStatus;

    public static SavingsAccount Create(
        int userId,
        SavingsType savingsType,
        decimal annualPercentageYield,
        string? accountName,
        int? fundingAccountId,
        decimal? targetAmount,
        DateTime? targetDate,
        DateTime? maturityDate,
        DateTime createdAt)
        => new(userId, savingsType, annualPercentageYield, accountName, fundingAccountId, targetAmount, targetDate, maturityDate, createdAt);

    /// <summary>Adds funds to the account balance.</summary>
    public ErrorOr<Success> Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            return SavingsErrors.InvalidDepositAmount;
        }

        if (AccountStatus == "Closed")
        {
            return SavingsErrors.AccountAlreadyClosed;
        }

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>Removes funds from the account balance.</summary>
    public ErrorOr<Success> Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            return SavingsErrors.InvalidDepositAmount;
        }

        if (AccountStatus == "Closed")
        {
            return SavingsErrors.AccountAlreadyClosed;
        }

        if (amount > Balance)
        {
            return SavingsErrors.InsufficientBalance;
        }

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>Closes the account, zeroing the balance.</summary>
    public ErrorOr<Success> Close()
    {
        if (AccountStatus == "Closed")
        {
            return SavingsErrors.AccountAlreadyClosed;
        }

        Balance = 0;
        AccountStatus = "Closed";
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }
}
