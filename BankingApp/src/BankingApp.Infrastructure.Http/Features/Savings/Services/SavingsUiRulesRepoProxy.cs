namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using BankingApp.Contracts.Features.Savings.Dtos;
using BankingApp.Contracts.Http;
using Domain.Aggregates.SavingsAggregate;
using Domain.Enums;
using Shared.Http;

public class SavingsUiRulesRepoProxy(ApiService apiService) : ISavingsUiRulesRepoProxy
{
    public async Task<string> GetDepositPreview(string depositAmountText, SavingsAccount selectedAccount)
    {
        var accountSnapshot = SavingsAccountSnapshotDto.FromAccount(selectedAccount);
        return await apiService.PostAsync<SavingsAccountSnapshotDto, string>(
            $"{ApiEndpoints.SavingsUiRules.DepositPreviewFull}?depositAmountText={Uri.EscapeDataString(depositAmountText)}",
            accountSnapshot);
    }

    public async Task<int> GetTotalPages(int totalCount, int pageSize)
    {
        return await apiService.GetAsync<int>(
            $"{ApiEndpoints.SavingsUiRules.TotalPagesFull}?totalCount={totalCount}&pageSize={pageSize}");
    }

    public async Task<decimal> GetWithdrawNetAmount(decimal requestedAmount, decimal penalty)
    {
        return await apiService.GetAsync<decimal>(
            $"{ApiEndpoints.SavingsUiRules.WithdrawNetAmountFull}?requestedAmount={requestedAmount}&penalty={penalty}");
    }

    public async Task<DepositFrequency> ParseDepositFrequency(string frequencyText)
    {
        return await apiService.GetAsync<DepositFrequency>(
            $"{ApiEndpoints.SavingsUiRules.ParseDepositFrequencyFull}?frequencyText={Uri.EscapeDataString(frequencyText)}");
    }

    public async Task<decimal> ParsePositiveAmount(string text)
    {
        return await apiService.GetAsync<decimal>(
            $"{ApiEndpoints.SavingsUiRules.ParsePositiveAmountFull}?text={Uri.EscapeDataString(text)}");
    }

    public async Task<Dictionary<string, string>> ValidateCreateAccount(ValidateCreateAccountRequest request)
    {
        return await apiService.PostAsync<ValidateCreateAccountRequest, Dictionary<string, string>>(
            ApiEndpoints.SavingsUiRules.ValidateCreateAccountFull,
            request);
    }
}
