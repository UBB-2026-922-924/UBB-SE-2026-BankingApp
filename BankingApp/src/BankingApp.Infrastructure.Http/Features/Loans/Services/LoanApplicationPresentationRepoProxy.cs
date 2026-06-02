namespace BankingApp.Infrastructure.Http.Features.Loans.Services;

using BankingApp.Contracts.Features.Loans.Dtos;
using BankingApp.Contracts.Http;
using Shared.Http;

public class LoanApplicationPresentationRepoProxy(ApiService apiService) : ILoanApplicationPresentationRepoProxy
{
    public Task<BuildApplicationOutcomeResponse> GetBuildApplicationOutcome(string? rejectionReason)
    {
        return apiService.GetAsync<BuildApplicationOutcomeResponse>(
            $"{ApiEndpoints.LoanApplicationPresentation.Base}?rejectionReason={Uri.EscapeDataString(rejectionReason ?? string.Empty)}");
    }
}
