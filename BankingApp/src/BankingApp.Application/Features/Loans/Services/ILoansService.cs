namespace BankingApp.Application.Features.Loans.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Features.Loans.Dtos;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;
using ErrorOr;

public interface ILoansService
{
    public Task<ErrorOr<IReadOnlyCollection<Loan>>> GetLoansByUserAsync(int userId, CancellationToken cancellationToken = default);

    public Task<ErrorOr<Loan>> GetLoanByIdAsync(int loanId, CancellationToken cancellationToken = default);

    public Task<ErrorOr<LoanApplicationResult>> SubmitApplicationAsync(LoanApplicationRequest request, CancellationToken cancellationToken = default);

    public ErrorOr<LoanEstimate> GetEstimate(LoanApplicationRequest request);

    public Task<ErrorOr<Success>> PayInstallmentAsync(int loanId, decimal? customAmount, CancellationToken cancellationToken = default);

    public Task<ErrorOr<IReadOnlyCollection<AmortizationRow>>> GetAmortizationAsync(int loanId, CancellationToken cancellationToken = default);
}

public sealed class LoanApplicationResult
{
    public required string Status { get; init; }
    public string? RejectionReason { get; init; }
}
