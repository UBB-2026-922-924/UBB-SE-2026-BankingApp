namespace BankingApp.Infrastructure.Http.Features.Loans.Services;

using BankingApp.Contracts.Http;
using Shared.Http;

public class LoanDialogStateRepoProxy(ApiService apiService) : ILoanDialogStateRepoProxy
{
    public Task<bool> GetShouldComputeEstimate(double desiredAmount, int preferredTermMonths, string purpose)
    {
        return apiService.GetAsync<bool>(
            $"{ApiEndpoints.LoanDialogState.Base}?desiredAmount={desiredAmount}&preferredTermMonths={preferredTermMonths}&purpose={Uri.EscapeDataString(purpose)}");
    }
}
