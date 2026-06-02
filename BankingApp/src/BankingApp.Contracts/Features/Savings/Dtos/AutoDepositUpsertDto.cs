namespace BankingApp.Contracts.Features.Savings.Dtos;

using System;
using Domain.Aggregates.SavingsAggregate.Entities;
using Domain.Enums;

/// <summary>Minimal request contract for creating or updating an auto-deposit schedule.</summary>
public class AutoDepositUpsertDto
{
    public int Id { get; set; }

    public int SavingsAccountId { get; set; }

    public decimal Amount { get; set; }

    public DepositFrequency Frequency { get; set; }

    public DateTime NextRunDate { get; set; }

    public bool IsActive { get; set; }

    public int? SourceAccountId { get; set; }

    public int? DayOfMonth { get; set; }

    public int? DayOfWeek { get; set; }

    public static AutoDepositUpsertDto FromAutoDeposit(AutoDeposit autoDeposit)
    {
        return new AutoDepositUpsertDto
        {
            Id = autoDeposit.Id,
            SavingsAccountId = autoDeposit.SavingsAccountId,
            Amount = autoDeposit.Amount,
            Frequency = autoDeposit.Frequency,
            NextRunDate = autoDeposit.NextRunDate,
            IsActive = autoDeposit.IsActive,
            SourceAccountId = autoDeposit.SourceAccountId,
            DayOfMonth = autoDeposit.DayOfMonth,
            DayOfWeek = autoDeposit.DayOfWeek,
        };
    }

    public AutoDeposit ToAutoDeposit()
    {
        return AutoDeposit.Reconstitute(Id, SavingsAccountId, Amount, Frequency, NextRunDate, IsActive, SourceAccountId, DayOfMonth, DayOfWeek, updatedAt: null);
    }
}
