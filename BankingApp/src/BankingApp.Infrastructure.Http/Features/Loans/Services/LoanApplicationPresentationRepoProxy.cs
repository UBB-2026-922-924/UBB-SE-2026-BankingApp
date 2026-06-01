namespace BankingApp.Infrastructure.Http.Features.Loans.Services;

using BankingApp.Contracts.Features.Loans.Dtos;
using BankingApp.Infrastructure.Http.Shared.Http;

public class LoanApplicationPresentationRepoProxy : ILoanApplicationPresentationRepoProxy
{
    private readonly ApiService _apiService;

    public LoanApplicationPresentationRepoProxy(ApiService apiService)
    {
        _apiService = apiService;
    }

    public Task<BuildApplicationOutcomeResponse?> GetBuildApplicationOutcome(string? rejectionReason)
    {
        return _apiService.GetAsync<BuildApplicationOutcomeResponse>(
            $"/api/loans/loan-application-presentation-outcome?rejectionReason={Uri.EscapeDataString(rejectionReason ?? string.Empty)}");
    }
}
