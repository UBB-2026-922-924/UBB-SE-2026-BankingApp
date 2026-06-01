namespace BankingApp.Infrastructure.Http.Features.Loans.Services;

using Shared.Http;

public class LoanDialogStateRepoProxy : ILoanDialogStateRepoProxy
{
    private readonly ApiService _apiService;

    public LoanDialogStateRepoProxy(ApiService apiService)
    {
        _apiService = apiService;
    }

    public Task<bool> GetShouldComputeEstimate(double desiredAmount, int preferredTermMonths, string purpose)
    {
        return _apiService.GetAsync<bool>(
            $"/api/loans/should-compute-estimate?desiredAmount={desiredAmount}&preferredTermMonths={preferredTermMonths}&purpose={Uri.EscapeDataString(purpose)}");
    }
}
