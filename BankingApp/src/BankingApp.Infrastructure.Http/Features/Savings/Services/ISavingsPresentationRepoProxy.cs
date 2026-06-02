using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Domain.Aggregates.SavingsAggregate;

namespace BankingApp.Infrastructure.Http.Features.Savings.Services
{
    public interface ISavingsPresentationRepoProxy
    {
        Task<string> GetTotalSavedAmount(IEnumerable<SavingsAccount> accounts);

        Task<string> GetNumberOfAccountsText(int accountCount);

        Task<string> GetBestInterestRate(IEnumerable<SavingsAccount> accounts);

        Task<bool> CheckClosePenaltyRisk(SavingsAccount selectedAccount);
    }
}
