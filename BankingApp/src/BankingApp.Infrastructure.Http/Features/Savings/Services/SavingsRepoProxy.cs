namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using System.Net;
using Contracts.Features.Investments;
using Contracts.Features.Savings.Dtos;
using Contracts.Http;
using Domain.Aggregates.SavingsAggregate;
using Domain.Aggregates.SavingsAggregate.Entities;
using Shared.Http;

public class SavingsRepoProxy(ApiService apiService) : ISavingsRepoProxy
{
    public async Task<ClosureResultDto> CloseSavingsAccountAsync(
        int accountId,
        int destinationAccountId,
        decimal transferAmount,
        decimal earlyClosurePenalty)
    {
        return await apiService.PostAsync<object, ClosureResultDto>(
            $"{ApiEndpoints.Savings.Base}/{accountId}/close?destinationAccountId={destinationAccountId}",
            new { });
    }

    public async Task<SavingsAccount> CreateSavingsAccountAsync(CreateSavingsAccountDto account, decimal apy)
    {
        return await apiService.PostAsync<CreateSavingsAccountDto, SavingsAccount>(
            ApiEndpoints.Savings.AccountsFull,
            account);
    }

    public async Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source)
    {
        return await apiService.PostAsync<object, DepositResponseDto>(
            $"{ApiEndpoints.Savings.Base}/{accountId}/deposit?amount={amount}&source={Uri.EscapeDataString(source)}",
            new { });
    }

    public async Task<List<SavingsAccount>> GetSavingsAccountsByUserIdAsync(
        int userId,
        bool includesClosed = false)
    {
        return await apiService.GetAsync<List<SavingsAccount>>(
            $"{ApiEndpoints.Savings.AccountsFull}?includesClosed={includesClosed}");
    }

    public async Task<AutoDeposit> GetAutoDepositAsync(int accountId)
    {
        try
        {
            return await apiService.GetAsync<AutoDeposit>($"{ApiEndpoints.Savings.Base}/{accountId}/auto-deposit");
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null!;
        }
    }

    public async Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId)
    {
        return await apiService.GetAsync<List<FundingSourceOption>>(ApiEndpoints.Savings.FundingSourcesFull);
    }

    public async Task<decimal> GetPenaltyDecimalFor(string penaltyCase)
    {
        return await apiService.GetAsync<decimal>($"{ApiEndpoints.Savings.Base}/penalty/rate/{penaltyCase}");
    }

    public async Task<GetTransactionsResponse> GetTransactionsAsync(
        int accountId,
        string filter = "",
        int page = 1,
        int pageSize = 20)
    {
        return await apiService.GetAsync<GetTransactionsResponse>(
            $"{ApiEndpoints.Savings.Base}/{accountId}/transactions?filter={Uri.EscapeDataString(filter)}&page={page}&pageSize={pageSize}");
    }

    public async Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId, int userId)
    {
        return await apiService.GetAsync<List<SavingsAccount>>(
            $"{ApiEndpoints.Savings.Base}/{currentAccountId}/valid-destinations");
    }

    public async Task SaveAutoDepositAsync(AutoDeposit autoDeposit)
    {
        await apiService.PostAsync<AutoDepositUpsertDto, object>(
            ApiEndpoints.Savings.AutoDepositFull,
            AutoDepositUpsertDto.FromAutoDeposit(autoDeposit));
    }

    public async Task<WithdrawResponseDto> WithdrawAsync(
        int accountId,
        decimal amount,
        string destinationLabel,
        decimal earlyWithdrawalPenalty)
    {
        return await apiService.PostAsync<object, WithdrawResponseDto>(
            $"{ApiEndpoints.Savings.Base}/{accountId}/withdraw?amount={amount}&destinationLabel={Uri.EscapeDataString(destinationLabel)}",
            new { });
    }
}
