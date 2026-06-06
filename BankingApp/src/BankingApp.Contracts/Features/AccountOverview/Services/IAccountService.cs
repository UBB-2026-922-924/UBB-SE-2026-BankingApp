namespace BankingApp.Contracts.Features.AccountOverview.Services;

using System;
using System.Collections.Generic;
using System.Text;
using Domain.Aggregates.AccountAggregate;
using ErrorOr;

public interface IAccountService
{
    public Task<ErrorOr<List<Account>>> GetAccountsAsync(CancellationToken cancellationToken = default);
}
