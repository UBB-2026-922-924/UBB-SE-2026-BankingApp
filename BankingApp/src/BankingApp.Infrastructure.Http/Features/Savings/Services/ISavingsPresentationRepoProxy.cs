namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using Domain.Aggregates.SavingsAggregate;

public interface ISavingsPresentationRepoProxy
{
    public Task<string> GetTotalSavedAmount(IEnumerable<SavingsAccount> accounts);

    public Task<string> GetNumberOfAccountsText(int accountCount);

    public Task<string> GetBestInterestRate(IEnumerable<SavingsAccount> accounts);

    public Task<bool> CheckClosePenaltyRisk(SavingsAccount selectedAccount);
}
