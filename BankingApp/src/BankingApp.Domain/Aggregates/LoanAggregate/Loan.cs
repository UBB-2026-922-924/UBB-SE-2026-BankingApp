namespace BankingApp.Domain.Aggregates.LoanAggregate;

using System;
using System.Collections.Generic;
using Entities;
using Common.Errors;
using Common.Primitives;
using Enums;
using ErrorOr;

public sealed class Loan : AggregateRoot<int>
{
    private readonly List<AmortizationRow> _amortizationRows = [];

    private Loan()
    {
    }

    private Loan(int userId, LoanType loanType, decimal principal, decimal interestRate, decimal monthlyInstallment, int termInMonths, DateTime startDate)
    {
        UserId = userId;
        LoanType = loanType;
        Principal = principal;
        OutstandingBalance = principal;
        InterestRate = interestRate;
        MonthlyInstallment = monthlyInstallment;
        TermInMonths = termInMonths;
        RemainingMonths = termInMonths;
        LoanStatus = LoanStatus.Active;
        StartDate = startDate;
    }

    public int UserId { get; private set; }
    public LoanType LoanType { get; private set; }
    public decimal Principal { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public decimal InterestRate { get; private set; }
    public decimal MonthlyInstallment { get; private set; }
    public int RemainingMonths { get; private set; }
    public LoanStatus LoanStatus { get; private set; }
    public int TermInMonths { get; private set; }
    public DateTime StartDate { get; private set; }

    public IReadOnlyCollection<AmortizationRow> AmortizationRows => _amortizationRows.AsReadOnly();

    public static Loan Create(int userId, LoanType loanType, decimal principal, decimal interestRate, decimal monthlyInstallment, int termInMonths, DateTime startDate)
        => new(userId, loanType, principal, interestRate, monthlyInstallment, termInMonths, startDate);

    /// <summary>Rebuilds a Loan from persisted storage state (infrastructure use only).</summary>
    public static Loan Reconstitute(int id, int userId, LoanType loanType, decimal principal, decimal outstandingBalance, decimal interestRate, decimal monthlyInstallment, int remainingMonths, LoanStatus loanStatus, int termInMonths, DateTime startDate)
        => new()
        {
            Id = id,
            UserId = userId,
            LoanType = loanType,
            Principal = principal,
            OutstandingBalance = outstandingBalance,
            InterestRate = interestRate,
            MonthlyInstallment = monthlyInstallment,
            RemainingMonths = remainingMonths,
            LoanStatus = loanStatus,
            TermInMonths = termInMonths,
            StartDate = startDate,
        };

    /// <summary>Applies a payment installment, reducing the outstanding balance and remaining months.</summary>
    public ErrorOr<Success> PayInstallment(decimal amount)
    {
        if (LoanStatus == LoanStatus.Passed)
        {
            return LoanErrors.LoanAlreadyClosed;
        }

        if (amount <= 0)
        {
            return LoanErrors.InvalidPaymentAmount;
        }

        OutstandingBalance -= amount;
        RemainingMonths--;

        if (OutstandingBalance <= 0 || RemainingMonths <= 0)
        {
            OutstandingBalance = 0;
            RemainingMonths = 0;
            LoanStatus = LoanStatus.Passed;
        }

        return Result.Success;
    }
}
