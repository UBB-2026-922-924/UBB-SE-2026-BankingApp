namespace BankingApp.Application.Features.AccountOverview.Services;

using System.Collections.Generic;
using BankingApp.Domain.Aggregates.AccountAggregate;

public interface IAccountService
{
    public Task<IReadOnlyCollection<Account>> GetAccountsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}

