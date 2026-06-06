namespace BankingApp.Application.Features.AccountOverview.Services;

using System.Collections.Generic;
using Domain.Aggregates.AccountAggregate;
using Domain.Repositories;

public sealed class AccountService(IAccountRepository accountRepository) : IAccountService
{
    public async Task<IReadOnlyCollection<Account>> GetAccountsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await accountRepository.ListByUserIdAsync(userId, cancellationToken);
    }
}
