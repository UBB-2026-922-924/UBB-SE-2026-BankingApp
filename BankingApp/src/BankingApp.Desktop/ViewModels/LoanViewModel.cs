namespace BankingApp.Desktop.ViewModels;

using Domain.Aggregates.LoanAggregate;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class LoanViewModel : ObservableObject
{
    public LoanViewModel(Loan loan, double repaymentProgress)
    {
        Loan = loan;
        RepaymentProgress = repaymentProgress;
    }

    public Loan Loan { get; }

    public double RepaymentProgress { get; }

    public int PaidInstallments => Loan.TermInMonths - Loan.RemainingMonths;
}