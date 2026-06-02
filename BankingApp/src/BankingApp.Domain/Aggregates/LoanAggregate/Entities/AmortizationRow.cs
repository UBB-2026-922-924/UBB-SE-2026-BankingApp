namespace BankingApp.Domain.Aggregates.LoanAggregate.Entities;

using System;
using Common.Primitives;

/// <summary>Represents a single row from a loan amortization schedule.</summary>
public sealed class AmortizationRow : Entity<int>
{
    private AmortizationRow()
    {
    }

    private AmortizationRow(int loanId, int installmentNumber, DateTime dueDate, decimal principalPortion, decimal interestPortion, decimal remainingBalance)
    {
        LoanId = loanId;
        InstallmentNumber = installmentNumber;
        DueDate = dueDate;
        PrincipalPortion = principalPortion;
        InterestPortion = interestPortion;
        RemainingBalance = remainingBalance;
    }

    public int LoanId { get; private set; }
    public int InstallmentNumber { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal PrincipalPortion { get; private set; }
    public decimal InterestPortion { get; private set; }
    public decimal RemainingBalance { get; private set; }
    public bool IsCurrent { get; private set; }

    public static AmortizationRow Create(int loanId, int installmentNumber, DateTime dueDate, decimal principalPortion, decimal interestPortion, decimal remainingBalance)
        => new(loanId, installmentNumber, dueDate, principalPortion, interestPortion, remainingBalance);

    /// <summary>Rebuilds an AmortizationRow from persisted storage state (infrastructure use only).</summary>
    public static AmortizationRow Reconstitute(int id, int loanId, int installmentNumber, DateTime dueDate, decimal principalPortion, decimal interestPortion, decimal remainingBalance, bool isCurrent)
        => new()
        {
            Id = id,
            LoanId = loanId,
            InstallmentNumber = installmentNumber,
            DueDate = dueDate,
            PrincipalPortion = principalPortion,
            InterestPortion = interestPortion,
            RemainingBalance = remainingBalance,
            IsCurrent = isCurrent,
        };

    public void MarkAsCurrent() => IsCurrent = true;

    public void ClearCurrent() => IsCurrent = false;
}
