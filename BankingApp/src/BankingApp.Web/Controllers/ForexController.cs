namespace BankingApp.Web.Controllers;

using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.Forex.Dtos;
using Contracts.Features.Forex.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Forex;

[Authorize]
public class ForexController(
    IForexService forexService,
    IBillPaymentService billPaymentService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ErrorOr<List<AccountDto>> accountsResult = await billPaymentService.GetAccountsAsync(ct);
        if (accountsResult.IsError)
        {
            TempData["Error"] = "Unable to load your accounts. Please try again.";
            return View(new ForexExchangeModel());
        }

        ForexExchangeModel viewModel = new()
        {
            Accounts = accountsResult.Value,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(ForexExchangeModel viewModel, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await RepopulateAccountsAsync(viewModel, ct);
            return View("Index", viewModel);
        }

        if (string.Equals(viewModel.FromCurrency, viewModel.ToCurrency, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Source and target currencies must be different.");
            await RepopulateAccountsAsync(viewModel, ct);
            return View("Index", viewModel);
        }

        Task<ErrorOr<ForexRatePreviewResponse>> previewTask =
            forexService.GetPreviewAsync(viewModel.FromCurrency, viewModel.ToCurrency, viewModel.Amount, ct);
        Task<ErrorOr<List<AccountDto>>> accountsTask = billPaymentService.GetAccountsAsync(ct);

        await Task.WhenAll(previewTask, accountsTask);

        ErrorOr<ForexRatePreviewResponse> previewResult = await previewTask;
        if (previewResult.IsError)
        {
            ModelState.AddModelError(string.Empty, "Unable to fetch exchange rate. Please try again.");
            await RepopulateAccountsAsync(viewModel, ct);
            return View("Index", viewModel);
        }

        ErrorOr<List<AccountDto>> accountsResult = await accountsTask;
        AccountDto? sourceAccount = accountsResult.IsError
            ? null
            : accountsResult.Value.FirstOrDefault(a => a.Id == viewModel.SelectedSourceAccountId);
        AccountDto? targetAccount = accountsResult.IsError
            ? null
            : accountsResult.Value.FirstOrDefault(a => a.Id == viewModel.SelectedTargetAccountId);

        ForexRatePreviewResponse preview = previewResult.Value;

        ForexPreviewModel previewVm = new()
        {
            SourceAccountId = viewModel.SelectedSourceAccountId,
            TargetAccountId = viewModel.SelectedTargetAccountId,
            FromCurrency = viewModel.FromCurrency,
            ToCurrency = viewModel.ToCurrency,
            Amount = viewModel.Amount,
            ExchangeRate = preview.ExchangeRate,
            ConvertedAmount = preview.TargetAmount,
            Commission = preview.Commission,
            SourceAccountIban = sourceAccount?.Iban ?? string.Empty,
            TargetAccountIban = targetAccount?.Iban ?? string.Empty,
        };

        return View("Preview", previewVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Execute(ForexPreviewModel viewModel, CancellationToken ct)
    {
        ForexTransactionRequest request = new()
        {
            SourceAccountId = viewModel.SourceAccountId,
            TargetAccountId = viewModel.TargetAccountId,
            SourceCurrency = viewModel.FromCurrency,
            TargetCurrency = viewModel.ToCurrency,
            SourceAmount = viewModel.Amount,
        };

        ErrorOr<ForexTransactionResponse> result = await forexService.ExecuteAsync(request, ct);

        if (result.IsError)
        {
            TempData["Error"] = result.FirstError.Description;
            return RedirectToAction(nameof(Index));
        }

        ForexTransactionResponse response = result.Value;
        TempData["Success"] =
            $"Exchange completed! {viewModel.FromCurrency} {viewModel.Amount:N2} → " +
            $"{viewModel.ToCurrency} {response.TargetAmount:N2} at rate {response.ExchangeRate:N6}.";

        return RedirectToAction(nameof(History));
    }

    public async Task<IActionResult> History(CancellationToken ct)
    {
        ErrorOr<List<ForexTransactionResponse>> result = await forexService.GetHistoryAsync(ct);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load exchange history. Please try again.";
            return View(new ForexHistoryModel());
        }

        ForexHistoryModel viewModel = new()
        {
            Transactions = result.Value.ConvertAll(t => new ForexHistoryRowModel
            {
                Id = t.Id,
                SourceCurrency = t.SourceCurrency,
                TargetCurrency = t.TargetCurrency,
                TargetAmount = t.TargetAmount,
                ExchangeRate = t.ExchangeRate,
                Commission = t.Commission,
                Status = t.Status.ToString(),
            })
        };

        return View(viewModel);
    }

    private async Task RepopulateAccountsAsync(ForexExchangeModel vm, CancellationToken ct)
    {
        ErrorOr<List<AccountDto>> result = await billPaymentService.GetAccountsAsync(ct);
        vm.Accounts = result.IsError ? [] : result.Value;
    }
}
