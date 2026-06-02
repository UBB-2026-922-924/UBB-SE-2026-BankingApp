using Microsoft.AspNetCore.Mvc;
using BankingApp.Web.ViewModels.Statistics;

namespace BankingApp.Web.Controllers
{
    using Application.Features.Statistics.Services;
    using Contracts.Features.Statistics.Dtos;

    public class StatisticsController : Controller
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                SpendingByCategoryResponse? spendingByCategory = await _statisticsService.GetSpendingByCategoryAsync();
                IncomeVsExpensesResponse? incomeVsExpenses = await _statisticsService.GetIncomeVsExpensesAsync();
                BalanceTrendsResponse? balanceTrends = await _statisticsService.GetBalanceTrendsAsync();
                TopRecipientsResponse? topRecipients = await _statisticsService.GetTopRecipientsAsync();

                var viewModel = new StatisticsViewModel
                {
                    SpendingByCategory = spendingByCategory,
                    IncomeVsExpenses = incomeVsExpenses,
                    BalanceTrends = balanceTrends,
                    TopRecipients = topRecipients
                };

                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Index", "Auth");
            }
        }
    }
}
