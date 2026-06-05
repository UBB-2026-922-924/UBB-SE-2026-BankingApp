namespace BankingApp.Web.Controllers;

using System.Globalization;
using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Features.Beneficiaries.Services;
using Contracts.Features.Transfers.Dtos;
using Contracts.Features.Transfers.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Transfers;

[Authorize]
public class TransfersController(
    ITransferService transferService,
    IBeneficiaryService beneficiaryService) : Controller
{
    public IActionResult Index() => RedirectToAction(nameof(New));

    public async Task<IActionResult> New(int? beneficiaryId, CancellationToken ct = default)
    {
        if (!beneficiaryId.HasValue)
        {
            return View(new TransferNewModel());
        }

        ErrorOr<BeneficiaryDto> beneficiaryResult = await beneficiaryService.GetByIdAsync(beneficiaryId.Value, ct);
        if (beneficiaryResult.IsError)
        {
            TempData["Error"] = "Unable to open transfer for the selected beneficiary.";
            return RedirectToAction("Index", "Beneficiaries");
        }

        Task<ErrorOr<TransferIbanValidationResponse>> validationTask =
            transferService.ValidateIbanAsync(
                new TransferIbanValidationRequest { Iban = beneficiaryResult.Value.Iban ?? string.Empty },
                ct);
        Task<ErrorOr<List<TransferAccountSelectionResponse>>> accountsTask = transferService.GetAccountsAsync(ct);

        await Task.WhenAll(validationTask, accountsTask);

        ErrorOr<TransferIbanValidationResponse> validationResult = await validationTask;
        ErrorOr<List<TransferAccountSelectionResponse>> accountsResult = await accountsTask;

        if (validationResult.IsError || !validationResult.Value.IsValid || accountsResult.IsError)
        {
            TempData["Error"] = "Unable to prepare a transfer for the selected beneficiary.";
            return RedirectToAction("Index", "Beneficiaries");
        }

        return View(
            "ValidateIban",
            new TransferIbanValidatedModel
            {
                RecipientIban = beneficiaryResult.Value.Iban ?? string.Empty,
                RecipientName = beneficiaryResult.Value.Name ?? string.Empty,
                RecipientBankName = string.IsNullOrWhiteSpace(beneficiaryResult.Value.BankName)
                    ? validationResult.Value.BankName
                    : beneficiaryResult.Value.BankName,
                Accounts = accountsResult.Value,
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidateIban(TransferNewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View("New", model);
        }

        Task<ErrorOr<TransferIbanValidationResponse>> validationTask =
            transferService.ValidateIbanAsync(new TransferIbanValidationRequest { Iban = model.RecipientIban }, ct);
        Task<ErrorOr<List<TransferAccountSelectionResponse>>> accountsTask =
            transferService.GetAccountsAsync(ct);

        await Task.WhenAll(validationTask, accountsTask);

        ErrorOr<TransferIbanValidationResponse> validationResult = await validationTask;
        if (validationResult.IsError || !validationResult.Value.IsValid)
        {
            ModelState.AddModelError(string.Empty, "The IBAN is invalid. Please check and try again.");
            return View("New", model);
        }

        ErrorOr<List<TransferAccountSelectionResponse>> accountsResult = await accountsTask;
        if (accountsResult.IsError)
        {
            TempData["Error"] = "Unable to load your accounts. Please try again.";
            return View("New", model);
        }

        TransferIbanValidatedModel viewModel = new()
        {
            RecipientIban = model.RecipientIban,
            RecipientBankName = validationResult.Value.BankName,
            Accounts = accountsResult.Value,
        };

        return View("ValidateIban", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(TransferIbanValidatedModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await RepopulateAccountsAsync(model, ct);
            return View("ValidateIban", model);
        }

        ErrorOr<List<TransferAccountSelectionResponse>> accountsResult = await transferService.GetAccountsAsync(ct);
        TransferAccountSelectionResponse? sourceAccount = accountsResult.IsError
            ? null
            : accountsResult.Value.FirstOrDefault(a => a.Id == model.SelectedAccountId);

        string sourceCurrency = sourceAccount?.Currency ?? string.Empty;

        TransferPreviewModel previewVm = new()
        {
            SourceAccountId = model.SelectedAccountId,
            SourceAccountIban = sourceAccount?.Iban ?? string.Empty,
            SourceCurrency = sourceCurrency,
            RecipientName = model.RecipientName,
            RecipientIban = model.RecipientIban,
            RecipientBankName = model.RecipientBankName,
            Amount = model.Amount,
            Currency = model.Currency,
            Reference = model.Reference,
        };

        if (!string.IsNullOrEmpty(sourceCurrency) && !string.IsNullOrEmpty(model.Currency) && model.Amount > 0)
        {
            ErrorOr<TransferForexPreviewResponse> fxResult =
                await transferService.GetFxPreviewAsync(sourceCurrency, model.Currency, model.Amount, ct);

            if (fxResult.IsError)
            {
                ModelState.AddModelError(string.Empty, "Unable to fetch forex rate. Please try again.");
                await RepopulateAccountsAsync(model, ct);
                return View("ValidateIban", model);
            }

            if (fxResult.Value.ExchangeRate != 1m)
            {
                previewVm.IsCrossCurrency = true;
            }
            
            previewVm.ExchangeRate = fxResult.Value.ExchangeRate;
            previewVm.ConvertedAmount = fxResult.Value.ConvertedAmount;
        }

        return View("Preview", previewVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(TransferPreviewModel model, CancellationToken ct)
    {
        CreateTransferRequest request = new()
        {
            SourceAccountId = model.SourceAccountId,
            RecipientName = model.RecipientName,
            RecipientIban = model.RecipientIban,
            Amount = model.Amount,
            Currency = model.Currency,
            Reference = model.Reference,
        };

        ErrorOr<TransferExecutionResponse> result = await transferService.ExecuteAsync(request, ct);

        if (result.IsError)
        {
            TempData["Error"] = result.FirstError.Description;
            return RedirectToAction(nameof(New));
        }

        TempData["Success"] =
            $"Transfer completed! Ref: {result.Value.TransactionRef} — " +
            $"{model.Currency} {model.Amount.ToString("N2", CultureInfo.InvariantCulture)} to {model.RecipientName}.";

        return RedirectToAction(nameof(History));
    }

    public async Task<IActionResult> History(CancellationToken ct)
    {
        ErrorOr<List<TransferResponse>> result = await transferService.GetHistoryAsync(ct);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load transfer history. Please try again.";
            return View(new TransferHistoryModel());
        }

        TransferHistoryModel viewModel = new()
        {
            Transfers = result.Value.ConvertAll(t => new TransferHistoryRowModel
            {
                Id = t.Id,
                RecipientIban = t.RecipientIban,
                RecipientName = t.RecipientName,
                Amount = t.Amount,
                Currency = t.Currency,
                Status = t.Status.ToString(),
                Reference = string.IsNullOrWhiteSpace(t.TransactionRef) ? "—" : t.TransactionRef,
                CreatedAt = t.CreatedAt,
            })
        };

        return View(viewModel);
    }

    private async Task RepopulateAccountsAsync(TransferIbanValidatedModel vm, CancellationToken ct)
    {
        ErrorOr<List<TransferAccountSelectionResponse>> result = await transferService.GetAccountsAsync(ct);
        vm.Accounts = result.IsError ? [] : result.Value;
    }
}
