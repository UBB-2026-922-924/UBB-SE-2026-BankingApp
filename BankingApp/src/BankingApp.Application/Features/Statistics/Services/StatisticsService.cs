using System;
using System.Threading.Tasks;
using BankingApp.Contracts.Features.Statistics.Dtos;

namespace BankingApp.Application.Features.Statistics.Services
{
    using Authentication.Services;

    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepoProxy _repoProxy;
        private readonly IAuthService _authService;

        public StatisticsService(IStatisticsRepoProxy repoProxy, IAuthService authService)
        {
            _repoProxy = repoProxy;
            _authService = authService;
        }

        public Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetSpendingByCategoryAsync();
        }

        public Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetIncomeVsExpensesAsync();
        }

        public Task<BalanceTrendsResponse?> GetBalanceTrendsAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetBalanceTrendsAsync();
        }

        public Task<TopRecipientsResponse?> GetTopRecipientsAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetTopRecipientsAsync();
        }

        private void EnsureAuthenticatedSession()
        {
            if (!_authService.IsAuthenticated())
            {
                throw new UnauthorizedAccessException("An authenticated session is required.");
            }
        }
    }
}

