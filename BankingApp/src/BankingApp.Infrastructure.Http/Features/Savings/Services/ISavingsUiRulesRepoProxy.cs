namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using BankingApp.Contracts.Features.Savings.Dtos;
using Domain.Enums;
using Domain.Aggregates.SavingsAggregate;

public interface ISavingsUiRulesRepoProxy
{
    public Task<decimal> ParsePositiveAmount(string text);

    public Task<string> GetDepositPreview(string depositAmountText, SavingsAccount selectedAccount);

    public Task<decimal> GetWithdrawNetAmount(decimal requestedAmount, decimal penalty);

    public Task<DepositFrequency> ParseDepositFrequency(string frequencyText);

    public Task<int> GetTotalPages(int totalCount, int pageSize);

    public Task<Dictionary<string, string>> ValidateCreateAccount(ValidateCreateAccountRequest request);
}
