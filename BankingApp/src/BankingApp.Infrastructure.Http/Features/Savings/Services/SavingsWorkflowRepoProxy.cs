namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using Contracts.Features.Investments;
using Contracts.Features.Savings.Dtos;
using Contracts.Http;
using Domain.Aggregates.SavingsAggregate;
using Shared.Http;

public class SavingsWorkflowRepoProxy(ApiService apiService) : ISavingsWorkflowRepoProxy
{
    public async Task<string> BuildWithdrawResultMessage(WithdrawResponseDto response)
    {
        return await apiService.PostAsync<WithdrawResponseDto, string>(
            ApiEndpoints.SavingsWorkflow.WithdrawResultMessageFull,
            response);
    }

    public async Task<bool> CanMoveToNextPage(int currentPage, int totalPages)
    {
        return await apiService.GetAsync<bool>(
            $"{ApiEndpoints.SavingsWorkflow.CanMoveNextFull}?currentPage={currentPage}&totalPages={totalPages}");
    }

    public async Task<bool> CanMoveToPreviousPage(int currentPage)
    {
        return await apiService.GetAsync<bool>(
            $"{ApiEndpoints.SavingsWorkflow.CanMovePreviousFull}?currentPage={currentPage}");
    }

    public async Task<int> GetDefaultCloseDestinationId(IEnumerable<SavingsAccount> destinationAccounts)
    {
        var accountSnapshots = destinationAccounts
            .Select(SavingsAccountSnapshotDto.FromAccount)
            .ToList();

        return await apiService.PostAsync<IEnumerable<SavingsAccountSnapshotDto>, int>(
            ApiEndpoints.SavingsWorkflow.DefaultCloseDestinationFull,
            accountSnapshots);
    }

    public async Task<FundingSourceOption> GetDefaultFundingSource(IEnumerable<FundingSourceOption> fundingSources)
    {
        return await apiService.PostAsync<IEnumerable<FundingSourceOption>, FundingSourceOption>(
            ApiEndpoints.SavingsWorkflow.DefaultFundingSourceFull,
            fundingSources);
    }

    public async Task<ValidationResponse> ValidateCloseConfirmation(bool userConfirmed, int destinationId)
    {
        return await apiService.GetAsync<ValidationResponse>(
            $"{ApiEndpoints.SavingsWorkflow.ValidateCloseFull}?userConfirmed={userConfirmed}&destinationId={destinationId}");
    }

    public async Task<ValidationResponse> ValidateWithdrawRequest(decimal amount, FundingSourceOption? destination)
    {
        return await apiService.PostAsync<FundingSourceOption?, ValidationResponse>(
            $"{ApiEndpoints.SavingsWorkflow.ValidateWithdrawFull}?amount={amount}",
            destination);
    }
}
