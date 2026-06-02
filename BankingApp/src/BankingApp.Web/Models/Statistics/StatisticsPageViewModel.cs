namespace BankingApp.Web.Models.Statistics;

using BankingApp.Contracts.Features.Statistics.Dtos;

public sealed class StatisticsPageViewModel
{
    public SpendingByCategoryResponse? SpendingByCategory { get; init; }

    public IncomeVsExpensesResponse? IncomeVsExpenses { get; init; }

    public BalanceTrendsResponse? BalanceTrends { get; init; }

    public TopRecipientsResponse? TopRecipients { get; init; }
}
