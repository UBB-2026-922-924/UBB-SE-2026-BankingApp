using BankingApp.Contracts.Features.Statistics.Dtos;

namespace BankingApp.Web.ViewModels.Statistics
{
    public class StatisticsViewModel
    {
        public SpendingByCategoryResponse? SpendingByCategory { get; set; }
        public IncomeVsExpensesResponse? IncomeVsExpenses { get; set; }
        public BalanceTrendsResponse? BalanceTrends { get; set; }
        public TopRecipientsResponse? TopRecipients { get; set; }
    }
}
