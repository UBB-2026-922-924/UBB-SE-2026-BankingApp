namespace BankingApp.Infrastructure.Http.Features.Loans.Services;

public interface ILoanDialogStateRepoProxy
{
    public Task<bool> GetShouldComputeEstimate(double desiredAmount, int preferredTermMonths, string purpose);
}
