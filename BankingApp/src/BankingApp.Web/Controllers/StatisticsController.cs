namespace BankingApp.Web.Controllers;

using Infrastructure.Http.Features.Statistics.Services;
using Models.Statistics;
using Microsoft.AspNetCore.Mvc;

public class StatisticsController(IStatisticsRepoProxy statisticsRepoProxy) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var viewModel = new StatisticsPageViewModel
            {
                SpendingByCategory = await statisticsRepoProxy.GetSpendingByCategoryAsync(),
                IncomeVsExpenses = await statisticsRepoProxy.GetIncomeVsExpensesAsync(),
                BalanceTrends = await statisticsRepoProxy.GetBalanceTrendsAsync(),
                TopRecipients = await statisticsRepoProxy.GetTopRecipientsAsync(),
            };

            return View(viewModel);
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
            return View(new StatisticsPageViewModel());
        }
    }
}
