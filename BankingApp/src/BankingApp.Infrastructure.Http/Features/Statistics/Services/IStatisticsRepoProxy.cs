using System.Threading.Tasks;
using BankingApp.Contracts.Features.Statistics.Dtos;

namespace BankingApp.Infrastructure.Http.Features.Statistics.Services
{
    public interface IStatisticsRepoProxy
    {
        Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync();

        Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync();

        Task<BalanceTrendsResponse?> GetBalanceTrendsAsync();

        Task<TopRecipientsResponse?> GetTopRecipientsAsync();
    }
}
