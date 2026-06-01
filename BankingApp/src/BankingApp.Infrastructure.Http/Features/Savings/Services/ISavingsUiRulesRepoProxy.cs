namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using BankingApp.Contracts.Features.Savings.Dtos;
using Domain.Enums;
using Domain.Aggregates.SavingsAggregate;

public interface ISavingsUiRulesRepoProxy
{
    Task<decimal> ParsePositiveAmount(string text);

    Task<string> GetDepositPreview(string depositAmountText, SavingsAccount selectedAccount);

    Task<decimal> GetWithdrawNetAmount(decimal requestedAmount, decimal penalty);

    Task<DepositFrequency> ParseDepositFrequency(string frequencyText);

    Task<int> GetTotalPages(int totalCount, int pageSize);

    Task<Dictionary<string, string>> ValidateCreateAccount(ValidateCreateAccountRequest request);
}
