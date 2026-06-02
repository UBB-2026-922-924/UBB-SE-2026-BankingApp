using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Contracts.Features.Savings.Dtos;
using BankingApp.Domain.Aggregates.InvestmentAggregate;
using BankingApp.Domain.Aggregates.SavingsAggregate;

namespace BankingApp.Infrastructure.Http.Features.Savings.Services
{
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
}
