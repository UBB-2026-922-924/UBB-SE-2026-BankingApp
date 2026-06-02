namespace BankingApp.Web.Controllers;

using Contracts.Features.Investments.Dtos;
using Contracts.Http;
using Infrastructure.Http.Features.Investments.Services;
using Models.Investments;
using Microsoft.AspNetCore.Mvc;

public class InvestmentsAndTradingController(IInvestmentsRepoProxy investmentsRepoProxy) : Controller
{
    private const decimal TradeFeeRate = 0.015m;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            PortfolioDto portfolio = await GetPortfolioAsync();
            return View(new InvestmentsPageViewModel { Portfolio = portfolio });
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
            return View(new InvestmentsPageViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Trade()
    {
        try
        {
            PortfolioDto portfolio = await GetPortfolioAsync();
            return View(new TradePageViewModel { WalletBalance = portfolio.TotalValue });
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
            return View(new TradePageViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExecuteTrade(string ticker, string actionType, decimal quantity)
    {
        decimal executionPrice = ticker switch
        {
            "BTC" => 65000.00m,
            "ETH" => 2550.00m,
            "SOL" => 145.00m,
            _ => 0m,
        };

        decimal fees = Math.Round(quantity * executionPrice * TradeFeeRate, 2);

        try
        {
            await investmentsRepoProxy.PostAsync<ExecuteTradeRequest, object>(
                ApiEndpoints.Investments.TradeFull,
                new ExecuteTradeRequest
                {
                    Ticker = ticker,
                    ActionType = actionType,
                    Quantity = quantity,
                    PricePerUnit = executionPrice,
                    Fees = fees,
                });

            TempData["Success"] = "Trade submitted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception)
        {
            TempData["Error"] = $"Execution failure: {exception.Message}";
            return RedirectToAction(nameof(Trade));
        }
    }

    private async Task<PortfolioDto> GetPortfolioAsync()
    {
        PortfolioDto? portfolio = await investmentsRepoProxy.GetAsync<PortfolioDto>(ApiEndpoints.Investments.PortfolioFull);
        return portfolio ?? new PortfolioDto();
    }
}
