namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using BankingApp.Domain.Aggregates.SavingsAggregate;

public interface ISavingsPresentationRepoProxy
{
    Task<string> GetTotalSavedAmount(IEnumerable<SavingsAccount> accounts);

    Task<string> GetNumberOfAccountsText(int accountCount);

    Task<string> GetBestInterestRate(IEnumerable<SavingsAccount> accounts);

    Task<bool> CheckClosePenaltyRisk(SavingsAccount selectedAccount);
}
