namespace BankingApp.Desktop.ViewModels;

using Domain.Aggregates.LoanAggregate;
using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
///     Provides display data for a loan row.
/// </summary>
public partial class LoanViewModel : ObservableObject
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LoanViewModel" /> class.
    /// </summary>
    /// <param name="loan">The source loan.</param>
    /// <param name="repaymentProgress">The repayment progress percentage.</param>
    public LoanViewModel(Loan loan, double repaymentProgress)
    {
        Loan = loan;
        RepaymentProgress = repaymentProgress;
    }

    /// <summary>
    ///     Gets the source loan.
    /// </summary>
    public Loan Loan { get; }

    /// <summary>
    ///     Gets the repayment progress percentage.
    /// </summary>
    public double RepaymentProgress { get; }

    /// <summary>
    ///     Gets the number of paid installments.
    /// </summary>
    public int PaidInstallments => Loan.TermInMonths - Loan.RemainingMonths;
}
