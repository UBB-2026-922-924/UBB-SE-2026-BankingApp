using System;
using System.Threading.Tasks;
using BankingApp.Client.RepoProxies;
using BankingApp.Contracts.Features.Loans.Dtos;

namespace BankingApp.Infrastructure.Http.Features.Loans.Services
{
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
}
