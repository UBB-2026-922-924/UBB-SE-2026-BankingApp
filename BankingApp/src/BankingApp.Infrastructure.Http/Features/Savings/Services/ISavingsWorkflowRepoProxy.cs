namespace BankingApp.Infrastructure.Http.Features.Savings.Services;

using BankingApp.Contracts.Features.Savings.Dtos;
using Domain.Aggregates.SavingsAggregate;
using BankingApp.Contracts.Features.Investments;

public interface ISavingsWorkflowRepoProxy
{
    Task<FundingSourceOption> GetDefaultFundingSource(IEnumerable<FundingSourceOption> fundingSources);

    Task<int> GetDefaultCloseDestinationId(IEnumerable<SavingsAccount> destinationAccounts);

    Task<ValidationResponse> ValidateWithdrawRequest(decimal amount, FundingSourceOption? destination);

    Task<string> BuildWithdrawResultMessage(WithdrawResponseDto response);

    Task<ValidationResponse> ValidateCloseConfirmation(bool userConfirmed, int destinationId);

    Task<bool> CanMoveToNextPage(int currentPage, int totalPages);

    Task<bool> CanMoveToPreviousPage(int currentPage);
}
