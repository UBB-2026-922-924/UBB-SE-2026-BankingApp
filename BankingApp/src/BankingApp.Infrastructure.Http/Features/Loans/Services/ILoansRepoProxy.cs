namespace BankingApp.Infrastructure.Http.Features.Loans.Services;

using BankingApp.Contracts.Features.Loans.Dtos;
using Domain.Enums;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;

/// <summary>
/// RepoProxy for Loans (desktop -> HTTP API).
/// This is intentionally low-level (CRUD-ish) so business rules can live in desktop services.
/// </summary>
public interface ILoansRepoProxy
{
    public Task<List<Loan>> GetAllLoansAsync();

    public Task<Loan> GetLoanByIdAsync(int id);

    public Task<List<Loan>> GetLoansByUserAsync(int userId);

    public Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus);

    public Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType);

    public Task<int> CreateLoanApplicationAsync(LoanApplicationRequest request);

    public Task UpdateLoanApplicationStatusAsync(int applicationId, LoanApplicationStatus status, string? reason);

    public Task<int> CreateLoanAsync(Loan loan);

    public Task UpdateLoanAfterPaymentAsync(int loanId, decimal newBalance, int newRemainingMonths, LoanStatus newStatus);

    public Task<List<AmortizationRow>> GetAmortizationAsync(int loanId);

    public Task SaveAmortizationAsync(int loanId, List<AmortizationRow> rows);
}
