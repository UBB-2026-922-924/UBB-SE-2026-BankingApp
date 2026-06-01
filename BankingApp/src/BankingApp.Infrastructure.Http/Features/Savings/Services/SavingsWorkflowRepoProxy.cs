namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using Contracts.Features.Savings.Dtos;
using Domain.Aggregates.SavingsAggregate;
using Contracts.Features.Investments;
using Infrastructure.Http.Shared.Http;

public class SavingsWorkflowRepoProxy(ApiService apiService) : ISavingsWorkflowRepoProxy
{
    public async Task<string> BuildWithdrawResultMessage(WithdrawResponseDto response)
    {
        return await apiService.PostAsync<WithdrawResponseDto, string>("/api/savings-workflow/withdraw-result-message", response);
    }

    public async Task<bool> CanMoveToNextPage(int currentPage, int totalPages)
    {
        return await apiService.GetAsync<bool>($"/api/savings-workflow/can-move-next?currentPage={currentPage}&totalPages={totalPages}");
    }

    public async Task<bool> CanMoveToPreviousPage(int currentPage)
    {
        return await apiService.GetAsync<bool>($"/api/savings-workflow/can-move-previous?currentPage={currentPage}");
    }

    public async Task<int> GetDefaultCloseDestinationId(IEnumerable<SavingsAccount> destinationAccounts)
    {
        var accountSnapshots = destinationAccounts.Select(SavingsAccountSnapshotDto.FromAccount).ToList();
        return await apiService.PostAsync<IEnumerable<SavingsAccountSnapshotDto>, int>("/api/savings-workflow/default-close-destination", accountSnapshots);
    }

    public async Task<FundingSourceOption> GetDefaultFundingSource(IEnumerable<FundingSourceOption> fundingSources)
    {
        return await apiService.PostAsync<IEnumerable<FundingSourceOption>, FundingSourceOption>("/api/savings-workflow/default-funding-source", fundingSources);
    }

    public async Task<ValidationResponse> ValidateCloseConfirmation(bool userConfirmed, int destinationId)
    {
        return await apiService.GetAsync<ValidationResponse>($"/api/savings-workflow/validate-close?userConfirmed={userConfirmed}&destinationId={destinationId}");
    }

    public async Task<ValidationResponse> ValidateWithdrawRequest(decimal amount, FundingSourceOption? destination)
    {
        return await apiService.PostAsync<FundingSourceOption?, ValidationResponse>($"/api/savings-workflow/validate-withdraw?amount={amount}", destination);
    }
}
