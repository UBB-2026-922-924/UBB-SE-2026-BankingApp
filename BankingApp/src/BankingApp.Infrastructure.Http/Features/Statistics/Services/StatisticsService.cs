namespace BankingApp.Infrastructure.Http.Features.Statistics.Services;

using System;
using System.Collections.Generic;
using System.Text;
using BankingApp.Application.Features.Authentication.Services;
using BankingApp.Application.Shared.Http;
using BankingApp.Contracts.Features.Statistics.Dtos;
using BankingApp.Contracts.Features.Statistics.Services;
using ErrorOr;

public class StatisticsService(IApiClient apiClient) : IStatisticsService
{

    public Task<ErrorOr<SpendingByCategoryResponse>> GetSpendingByCategoryAsync(CancellationToken ct = default)
    {
        EnsureAuthenticatedSession();
        return apiClient.GetAsync<SpendingByCategoryResponse>("/api/statistics/spending-by-category", ct);
    }

    public Task<ErrorOr<IncomeVsExpensesResponse>> GetIncomeVsExpensesAsync(CancellationToken ct = default)
    {
        EnsureAuthenticatedSession();
        return apiClient.GetAsync<IncomeVsExpensesResponse>("/api/statistics/income-vs-expenses", ct);
    }

    public Task<ErrorOr<BalanceTrendsResponse>> GetBalanceTrendsAsync(CancellationToken ct = default)
    {
        EnsureAuthenticatedSession();
        return apiClient.GetAsync<BalanceTrendsResponse>("/api/statistics/balance-trends", ct);
    }

    public Task<ErrorOr<TopRecipientsResponse>> GetTopRecipientsAsync(CancellationToken ct = default)
    {
        EnsureAuthenticatedSession();
        return apiClient.GetAsync<TopRecipientsResponse>("/api/statistics/top-recipients", ct);
    }

    private void EnsureAuthenticatedSession()
    {
        //TODO no idea how to check if user is authenticated sorry
        /*
        if (!_authService.IsAuthenticated())
        {
            throw new UnauthorizedAccessException("An authenticated session is required.");
        }
        */
    }
}
