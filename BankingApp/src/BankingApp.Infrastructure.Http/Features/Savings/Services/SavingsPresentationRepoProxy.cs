namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using BankingApp.Contracts.Features.Savings.Dtos;
using BankingApp.Domain.Aggregates.SavingsAggregate;
using BankingApp.Infrastructure.Http.Shared.Http;

public class SavingsPresentationRepoProxy : ISavingsPresentationRepoProxy
{
    private readonly ApiService _apiService;

    public SavingsPresentationRepoProxy(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<bool> CheckClosePenaltyRisk(SavingsAccount selectedAccount)
    {
        var accountSnapshot = SavingsAccountSnapshotDto.FromAccount(selectedAccount);
        return await _apiService.PostAsync<SavingsAccountSnapshotDto, bool>("/api/savings-presentation/close-penalty-risk", accountSnapshot);
    }

    public async Task<string> GetBestInterestRate(IEnumerable<SavingsAccount> accounts)
    {
        var accountSnapshots = accounts.Select(SavingsAccountSnapshotDto.FromAccount).ToList();
        return await _apiService.PostAsync<IEnumerable<SavingsAccountSnapshotDto>, string>("/api/savings-presentation/best-interest-rate", accountSnapshots);
    }

    public async Task<string> GetNumberOfAccountsText(int accountCount)
    {
        return await _apiService.GetAsync<string>($"/api/savings-presentation/accounts-text/{accountCount}");
    }

    public async Task<string> GetTotalSavedAmount(IEnumerable<SavingsAccount> accounts)
    {
        var accountSnapshots = accounts.Select(SavingsAccountSnapshotDto.FromAccount).ToList();
        return await _apiService.PostAsync<IEnumerable<SavingsAccountSnapshotDto>, string>("/api/savings-presentation/total-saved", accountSnapshots);
    }
}
