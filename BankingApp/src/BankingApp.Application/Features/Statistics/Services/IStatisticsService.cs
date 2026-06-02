using System.Threading.Tasks;
using BankingApp.Contracts.Features.Statistics.Dtos;

namespace BankingApp.Application.Features.Statistics.Services
{
    public interface IStatisticsService
    {
        Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync();
        Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync();
        Task<BalanceTrendsResponse?> GetBalanceTrendsAsync();
        Task<TopRecipientsResponse?> GetTopRecipientsAsync();
    }
}

