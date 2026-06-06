namespace BankingApp.Web.Controllers;

using Contracts.Features.AccountOverview.Services;
using Domain.Aggregates.AccountAggregate;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class AccountsController(IAccountService accountService): Controller
{

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ErrorOr<List<Account>> accounts = await accountService.GetAccountsAsync();
        return View(accounts.Value);
    }
}
