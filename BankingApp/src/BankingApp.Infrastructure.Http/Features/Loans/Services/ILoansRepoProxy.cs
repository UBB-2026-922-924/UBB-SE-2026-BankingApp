namespace BankingApp.Infrastructure.Http.Features.Loans.Services;

using BankingApp.Contracts.Features.Loans.Dtos;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Aggregates.LoanAggregate;
using BankingApp.Domain.Aggregates.LoanAggregate.Entities;

/// <summary>
/// RepoProxy for Loans (desktop -> HTTP API).
/// This is intentionally low-level (CRUD-ish) so business rules can live in desktop services.
/// </summary>
public interface ILoansRepoProxy
{
    Task<List<Loan>> GetAllLoansAsync();

    Task<Loan> GetLoanByIdAsync(int id);

    Task<List<Loan>> GetLoansByUserAsync(int userId);

    Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus);

    Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType);

    Task<int> CreateLoanApplicationAsync(LoanApplicationRequest request);

    Task UpdateLoanApplicationStatusAsync(int applicationId, LoanApplicationStatus status, string? reason);

    Task<int> CreateLoanAsync(Loan loan);

    Task UpdateLoanAfterPaymentAsync(int loanId, decimal newBalance, int newRemainingMonths, LoanStatus newStatus);

    Task<List<AmortizationRow>> GetAmortizationAsync(int loanId);

    Task SaveAmortizationAsync(int loanId, List<AmortizationRow> rows);
}
