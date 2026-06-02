namespace BankingApp.Web.Controllers;

using Contracts.Features.AccountOverview.Dtos;
using Contracts.Features.AccountOverview.Services;
using Models.Dashboard;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>Renders the authenticated user's dashboard overview.</summary>
[Authorize]
public class DashboardController(IAccountOverviewService accountOverviewService) : Controller
{
    /// <summary>Displays the dashboard page.</summary>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ErrorOr<AccountOverviewDto> result = await accountOverviewService.GetDashboardAsync(cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Could not load dashboard data. Please try again.";
            return View(new DashboardModel());
        }

        AccountOverviewDto dto = result.Value;

        // Build view-ready models — all formatting logic lives in the model classes,
        // keeping the Razor view free of business / display logic.
        var model = new DashboardModel
        {
            UserSummary             = dto.CurrentUser ?? new UserSummaryDto(),
            Cards                   = dto.Cards.Select(c => new DashboardCardModel(c)).ToList(),
            RecentTransactions      = dto.RecentTransactions.Select(t => new DashboardTransactionModel(t)).ToList(),
            UnreadNotificationCount = dto.UnreadNotificationCount
        };

        // Make the notification count available to the shared layout so it can
        // render a badge in the navbar without a separate service call.
        ViewData["NotificationCount"] = model.UnreadNotificationCount;

        return View(model);
    }
}