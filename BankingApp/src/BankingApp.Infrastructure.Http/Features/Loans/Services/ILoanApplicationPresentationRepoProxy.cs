using System.Threading.Tasks;
using BankingApp.Contracts.Features.Loans.Dtos;

namespace BankingApp.Infrastructure.Http.Features.Loans.Services
{
    public interface ILoanApplicationPresentationRepoProxy
    {
        Task<BuildApplicationOutcomeResponse?> GetBuildApplicationOutcome(string? rejectionReason);
    }
}
