namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using BankingApp.Contracts.Features.Savings.Dtos;
using Domain.Aggregates.SavingsAggregate;
using BankingApp.Contracts.Features.Investments;

public interface ISavingsWorkflowRepoProxy
{
    public Task<FundingSourceOption> GetDefaultFundingSource(IEnumerable<FundingSourceOption> fundingSources);

    public Task<int> GetDefaultCloseDestinationId(IEnumerable<SavingsAccount> destinationAccounts);

    public Task<ValidationResponse> ValidateWithdrawRequest(decimal amount, FundingSourceOption? destination);

    public Task<string> BuildWithdrawResultMessage(WithdrawResponseDto response);

    public Task<ValidationResponse> ValidateCloseConfirmation(bool userConfirmed, int destinationId);

    public Task<bool> CanMoveToNextPage(int currentPage, int totalPages);

    public Task<bool> CanMoveToPreviousPage(int currentPage);
}
