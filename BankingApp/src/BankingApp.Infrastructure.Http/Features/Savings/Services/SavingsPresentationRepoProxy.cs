namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using BankingApp.Contracts.Features.Savings.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Domain.Aggregates.SavingsAggregate;
using Shared.Http;

public class SavingsPresentationRepoProxy(ApiService apiService) : ISavingsPresentationRepoProxy
{
    public async Task<bool> CheckClosePenaltyRisk(SavingsAccount selectedAccount)
    {
        var accountSnapshot = SavingsAccountSnapshotDto.FromAccount(selectedAccount);
        return await apiService.PostAsync<SavingsAccountSnapshotDto, bool>(
            ApiEndpoints.SavingsPresentation.ClosePenaltyRiskFull,
            accountSnapshot);
    }

    public async Task<string> GetBestInterestRate(IEnumerable<SavingsAccount> accounts)
    {
        var accountSnapshots = accounts
            .Select(SavingsAccountSnapshotDto.FromAccount)
            .ToList();

        return await apiService.PostAsync<IEnumerable<SavingsAccountSnapshotDto>, string>(
            ApiEndpoints.SavingsPresentation.BestInterestRateFull,
            accountSnapshots);
    }

    public async Task<string> GetNumberOfAccountsText(int accountCount)
    {
        return await apiService.GetAsync<string>(ApiEndpoints.SavingsPresentation.AccountsTextFull(accountCount));
    }

    public async Task<string> GetTotalSavedAmount(IEnumerable<SavingsAccount> accounts)
    {
        var accountSnapshots = accounts
            .Select(SavingsAccountSnapshotDto.FromAccount)
            .ToList();

        return await apiService.PostAsync<IEnumerable<SavingsAccountSnapshotDto>, string>(
            ApiEndpoints.SavingsPresentation.TotalSavedFull,
            accountSnapshots);
    }
}
