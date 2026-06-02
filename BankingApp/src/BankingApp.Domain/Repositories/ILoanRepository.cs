namespace BankingApp.Domain.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aggregates.LoanAggregate;
using Aggregates.LoanAggregate.Entities;
using Enums;

public interface ILoanRepository
{
    /// <summary>Gets all loans.</summary>
    public Task<IReadOnlyCollection<Loan>> GetAllLoansAsync(CancellationToken cancellationToken);

    /// <summary>Gets a loan by identifier.</summary>
    public Task<Loan?> GetLoanByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>Gets loans for a user.</summary>
    public Task<IReadOnlyCollection<Loan>> GetLoansByUserAsync(int userId, CancellationToken cancellationToken);

    /// <summary>Gets loans by status.</summary>
    public Task<IReadOnlyCollection<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus, CancellationToken cancellationToken);

    /// <summary>Gets loans by type.</summary>
    public Task<IReadOnlyCollection<Loan>> GetLoansByTypeAsync(LoanType loanType, CancellationToken cancellationToken);

    /// <summary>Saves an amortization schedule for a loan.</summary>
    public Task SaveAmortizationAsync(IReadOnlyCollection<AmortizationRow> rows, CancellationToken cancellationToken);

    /// <summary>Gets the amortization schedule for a loan.</summary>
    public Task<IReadOnlyCollection<AmortizationRow>> GetAmortizationAsync(int loanId, CancellationToken cancellationToken);

    /// <summary>Persists a new loan application and returns its assigned identifier.</summary>
    public Task<int> CreateLoanApplicationAsync(LoanApplication application, CancellationToken cancellationToken);

    /// <summary>Updates loan application decision status.</summary>
    public Task UpdateLoanApplicationStatusAsync(int id, LoanApplicationStatus loanApplicationStatus, string? reason, CancellationToken cancellationToken);

    /// <summary>Creates a new approved loan record and returns its identifier.</summary>
    public Task<int> CreateLoanAsync(Loan loan, CancellationToken cancellationToken);

    /// <summary>Updates balance and status after payment.</summary>
    public Task UpdateLoanAfterPaymentAsync(int id, decimal newBalance, int newRemainingMonths, LoanStatus newStatus, CancellationToken cancellationToken);
}
