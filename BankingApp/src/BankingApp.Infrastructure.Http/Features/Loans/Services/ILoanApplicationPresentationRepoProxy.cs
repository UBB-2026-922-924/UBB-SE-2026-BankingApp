namespace BankingApp.Infrastructure.Http.Features.Loans.Services;

using BankingApp.Contracts.Features.Loans.Dtos;

public interface ILoanApplicationPresentationRepoProxy
{
    public Task<BuildApplicationOutcomeResponse> GetBuildApplicationOutcome(string? rejectionReason);
}
