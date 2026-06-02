namespace BankingApp.Contracts.Features.Loans.Dtos;

using System;
using Domain.Aggregates.LoanAggregate;
using Domain.Enums;

/// <summary>Minimal request contract for creating an approved loan record.</summary>
public class LoanCreateDto
{
    public int UserId { get; set; }

    public LoanType LoanType { get; set; }

    public decimal Principal { get; set; }

    public decimal InterestRate { get; set; }

    public decimal MonthlyInstallment { get; set; }

    public int TermInMonths { get; set; }

    public DateTime StartDate { get; set; }

    public static LoanCreateDto FromLoan(Loan loan)
    {
        return new LoanCreateDto
        {
            UserId = loan.UserId,
            LoanType = loan.LoanType,
            Principal = loan.Principal,
            InterestRate = loan.InterestRate,
            MonthlyInstallment = loan.MonthlyInstallment,
            TermInMonths = loan.TermInMonths,
            StartDate = loan.StartDate,
        };
    }

    public Loan ToLoan()
    {
        return Loan.Create(UserId, LoanType, Principal, InterestRate, MonthlyInstallment, TermInMonths, StartDate);
    }
}
