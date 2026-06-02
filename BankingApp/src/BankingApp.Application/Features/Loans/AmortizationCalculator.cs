namespace BankingApp.Application.Features.Loans;

using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;

public static class AmortizationCalculator
{
    private const decimal ZeroDecimal = 0m;
    private const int FirstInstallmentNumber = 1;
    private const decimal OneDecimal = 1m;
    private const decimal MonthsPerYear = 12m;
    private const decimal PercentageScale = 100m;
    private const int CurrencyPrecisionDigits = 2;

    public static LoanEstimate ComputeEstimate(decimal amount, decimal annualRate, int termMonths)
    {
        decimal monthlyRate = annualRate / MonthsPerYear / PercentageScale;
        decimal monthlyInstallment;

        if (monthlyRate == ZeroDecimal)
        {
            monthlyInstallment = amount / termMonths;
        }
        else
        {
            monthlyInstallment = amount * monthlyRate * (decimal)Math.Pow((double)(OneDecimal + monthlyRate), termMonths) /
                                 ((decimal)Math.Pow((double)(OneDecimal + monthlyRate), termMonths) - OneDecimal);
        }

        monthlyInstallment = Math.Round(monthlyInstallment, CurrencyPrecisionDigits);
        decimal totalRepayable = Math.Round(monthlyInstallment * termMonths, CurrencyPrecisionDigits);

        return new LoanEstimate(annualRate, monthlyInstallment, totalRepayable);
    }

    public static decimal ComputeRepaymentProgress(decimal principal, decimal outstandingBalance)
    {
        if (principal == ZeroDecimal)
        {
            return ZeroDecimal;
        }

        return (principal - outstandingBalance) / principal * PercentageScale;
    }

    public static List<AmortizationRow> Generate(Loan loan)
    {
        var rows = new List<AmortizationRow>();

        decimal principal = loan.Principal;
        decimal annualRate = loan.InterestRate;
        int termInMonths = loan.TermInMonths;
        DateTime startDate = loan.StartDate;

        decimal monthlyRate = annualRate / MonthsPerYear / PercentageScale;
        decimal remainingBalance = principal;
        decimal monthlyInstallment;

        if (monthlyRate == ZeroDecimal)
        {
            monthlyInstallment = remainingBalance / termInMonths;
        }
        else
        {
            monthlyInstallment = remainingBalance * monthlyRate *
                                 (decimal)Math.Pow((double)(OneDecimal + monthlyRate), termInMonths) /
                                 ((decimal)Math.Pow((double)(OneDecimal + monthlyRate), termInMonths) - OneDecimal);
        }

        monthlyInstallment = Math.Round(monthlyInstallment, CurrencyPrecisionDigits);
        bool isCurrentMarked = false;

        for (int index = FirstInstallmentNumber; index <= termInMonths; index++)
        {
            DateTime dueDate = startDate.AddMonths(index);
            decimal interestPortion = Math.Round(remainingBalance * monthlyRate, CurrencyPrecisionDigits);
            decimal principalPortion = monthlyInstallment - interestPortion;

            if (index == termInMonths)
            {
                principalPortion = remainingBalance;
                monthlyInstallment = principalPortion + interestPortion;
            }

            remainingBalance -= principalPortion;

            if (remainingBalance < ZeroDecimal || index == termInMonths)
            {
                remainingBalance = ZeroDecimal;
            }

            var row = AmortizationRow.Create(
                loan.Id,
                index,
                dueDate,
                principalPortion,
                interestPortion,
                remainingBalance);

            if (!isCurrentMarked && dueDate.Date >= DateTime.Today)
            {
                row.MarkAsCurrent();
                isCurrentMarked = true;
            }

            rows.Add(row);
        }

        return rows;
    }
}

