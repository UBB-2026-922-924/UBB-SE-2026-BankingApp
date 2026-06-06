namespace BankingApp.Infrastructure.Http.Features.AccountOverview.Services;

using System;
using System.Collections.Generic;
using System.Text;
using Application.Shared.Http;
using Contracts.Features.AccountOverview.Services;
using Contracts.Http;
using Domain.Aggregates.AccountAggregate;
using ErrorOr;

public sealed class AccountService(IApiClient apiClient): IAccountService
{
    public Task<ErrorOr<List<Account>>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        return apiClient.GetAsync<List<Account>>(ApiEndpoints.AccountOverview.AccountsFull, cancellationToken);
    }
}
