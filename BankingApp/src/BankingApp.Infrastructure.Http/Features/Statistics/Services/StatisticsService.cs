namespace BankingApp.Infrastructure.Http.Features.Statistics.Services;

using System;
using System.Collections.Generic;
using System.Text;
using BankingApp.Application.Features.Authentication.Services;
using BankingApp.Application.Shared.Http;
using BankingApp.Contracts.Features.Statistics.Dtos;
using BankingApp.Contracts.Features.Statistics.Services;
using Contracts.Http;
using ErrorOr;

public sealed class StatisticsService(IApiClient apiClient) : IStatisticsService
{

    public Task<ErrorOr<SpendingByCategoryResponse>> GetSpendingByCategoryAsync(CancellationToken ct = default)
    {
        EnsureAuthenticatedSession();
        return apiClient.GetAsync<SpendingByCategoryResponse>(ApiEndpoints.Statistics.SpendingByCategoryFull, ct);
    }

    public Task<ErrorOr<IncomeVsExpensesResponse>> GetIncomeVsExpensesAsync(CancellationToken ct = default)
    {
        EnsureAuthenticatedSession();
        return apiClient.GetAsync<IncomeVsExpensesResponse>(ApiEndpoints.Statistics.IncomeVsExpensesFull, ct);
    }

    public Task<ErrorOr<BalanceTrendsResponse>> GetBalanceTrendsAsync(CancellationToken ct = default)
    {
        EnsureAuthenticatedSession();
        return apiClient.GetAsync<BalanceTrendsResponse>(ApiEndpoints.Statistics.BalanceTrendsFull, ct);
    }

    public Task<ErrorOr<TopRecipientsResponse>> GetTopRecipientsAsync(CancellationToken ct = default)
    {
        EnsureAuthenticatedSession();
        return apiClient.GetAsync<TopRecipientsResponse>(ApiEndpoints.Statistics.TopRecipientsFull, ct);
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
