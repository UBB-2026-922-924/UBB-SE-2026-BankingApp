namespace BankingApp.Infrastructure.Http.Features.Loans.Services;

using Application.Features.Loans.Services;
using Contracts.Features.Loans.Dtos;
using Contracts.Http;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;
using Domain.Enums;
using Shared.Http;

public class LoansRepoProxy(ApiService apiService) : ILoansRepoProxy
{
    public Task<List<Loan>> GetAllLoansAsync()
    {
        return apiService.GetAsync<List<Loan>>(ApiEndpoints.Loans.Base);
    }

    public Task<Loan> GetLoanByIdAsync(int id)
    {
        return apiService.GetAsync<Loan>(ApiEndpoints.Loans.ByIdFull(id));
    }

    public Task<List<Loan>> GetLoansByUserAsync(int userId)
    {
        return apiService.GetAsync<List<Loan>>(ApiEndpoints.Loans.Base);
    }

    public Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus)
    {
        return apiService.GetAsync<List<Loan>>($"{ApiEndpoints.Loans.Base}?status={loanStatus}");
    }

    public Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType)
    {
        return apiService.GetAsync<List<Loan>>($"{ApiEndpoints.Loans.Base}?type={loanType}");
    }

    public async Task<int> CreateLoanApplicationAsync(LoanApplicationRequest request)
    {
        LoanApplicationResult result =
            await apiService.PostAsync<LoanApplicationRequest, LoanApplicationResult>(
                ApiEndpoints.Loans.ApplicationsFull,
                request);

        return result.Status.Equals(LoanApplicationStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase)
            ? 1
            : 0;
    }

    public async Task UpdateLoanApplicationStatusAsync(int applicationId, LoanApplicationStatus status, string? reason)
    {
        string reasonParam = reason == null ? string.Empty : $"&reason={Uri.EscapeDataString(reason)}";
        await apiService.PutAsync<object, object>(
            $"{ApiEndpoints.Loans.ApplicationStatusFull(applicationId)}?status={status}{reasonParam}",
            new { });
    }

    public async Task<int> CreateLoanAsync(Loan loan)
    {
        int result = await apiService.PostAsync<LoanCreateDto, int>(
            ApiEndpoints.Loans.Base,
            LoanCreateDto.FromLoan(loan));
        return result;
    }

    public async Task UpdateLoanAfterPaymentAsync(
        int loanId,
        decimal newBalance,
        int newRemainingMonths,
        LoanStatus newStatus)
    {
        await apiService.PutAsync<object, object>(
            $"{ApiEndpoints.Loans.AfterPaymentFull(loanId)}?newBalance={newBalance}&newRemainingMonths={newRemainingMonths}&newStatus={newStatus}",
            new { });
    }

    public Task<List<AmortizationRow>> GetAmortizationAsync(int loanId)
    {
        return apiService.GetAsync<List<AmortizationRow>>(
            ApiEndpoints.Loans.AmortizationScheduleFull(loanId));
    }

    public async Task SaveAmortizationAsync(int loanId, List<AmortizationRow> rows)
    {
        await apiService.PostAsync<List<AmortizationRowUpsertDto>, object>(
            ApiEndpoints.Loans.AmortizationScheduleFull(loanId),
            rows.ConvertAll(AmortizationRowUpsertDto.FromAmortizationRow));
    }
}
