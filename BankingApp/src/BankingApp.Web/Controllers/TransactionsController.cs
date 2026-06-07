namespace BankingApp.Web.Controllers;

using System.Globalization;
using Contracts.Features.Transactions.Dtos;
using Contracts.Features.Transactions.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Transactions;

[Authorize]
public class TransactionsController(
    ITransactionService transactionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] TransactionHistoryPageViewModel model,
        [FromQuery] int? selectedTransactionId,
        CancellationToken ct)
    {
        Task<ErrorOr<TransactionHistoryResponse>> historyTask =
            transactionService.GetHistoryAsync(model.ToHistoryRequest(), ct);
        Task<ErrorOr<TransactionFilterMetadataResponse>> metadataTask =
            transactionService.GetFilterMetadataAsync(ct);

        await Task.WhenAll(historyTask, metadataTask);

        ErrorOr<TransactionHistoryResponse> historyResult = await historyTask;
        if (historyResult.IsError)
        {
            model.Transactions = [];
            model.StatusMessage = historyResult.FirstError.Description;
            model.IsSuccess = false;
        }
        else
        {
            model.Transactions = historyResult.Value.Transactions;
            if (historyResult.Value.AppliedFilters is not null)
            {
                model.ApplyFilters(historyResult.Value.AppliedFilters);
            }
            model.SelectedTransactionId = ResolveSelectedTransactionId(
                model.Transactions,
                selectedTransactionId ?? model.SelectedTransactionId);
        }

        ErrorOr<TransactionFilterMetadataResponse> metadataResult = await metadataTask;
        if (!metadataResult.IsError)
        {
            PopulateFilterOptions(model, metadataResult.Value);
        }

        string? statusMessage = TempData["StatusMessage"] as string;
        string? errorMessage = TempData["ErrorMessage"] as string;
        model.StatusMessage = statusMessage ?? errorMessage ?? model.StatusMessage;
        model.LastExportPath = TempData["LastExportPath"] as string ?? model.LastExportPath;
        model.IsSuccess = string.IsNullOrWhiteSpace(model.StatusMessage) || statusMessage != null;

        return View(model);
    }

    [HttpGet]
    public Task<IActionResult> ExportCsv(
        [FromQuery] TransactionHistoryPageViewModel model,
        CancellationToken ct) =>
        ExportAsync(model, TransactionExportFormats.Csv, ct);

    [HttpGet]
    public Task<IActionResult> ExportPdf(
        [FromQuery] TransactionHistoryPageViewModel model,
        CancellationToken ct) =>
        ExportAsync(model, TransactionExportFormats.Pdf, ct);

    [HttpGet]
    public Task<IActionResult> ExportXlsx(
        [FromQuery] TransactionHistoryPageViewModel model,
        CancellationToken ct) =>
        ExportAsync(model, TransactionExportFormats.Xlsx, ct);

    [HttpGet]
    public async Task<IActionResult> ExportReceipt(int transactionId, CancellationToken ct)
    {
        ErrorOr<TransactionExportResult> result =
            await transactionService.ExportReceiptAsync(transactionId, ct);

        if (result.IsError)
        {
            TempData["ErrorMessage"] = result.FirstError.Description;
        }
        else
        {
            TempData["StatusMessage"] = $"Receipt exported to {result.Value.FileName}.";
            TempData["LastExportPath"] = result.Value.FileName;
        }

        return RedirectToAction(nameof(Index), new { selectedTransactionId = transactionId });
    }

    private async Task<IActionResult> ExportAsync(
        TransactionHistoryPageViewModel model,
        string format,
        CancellationToken ct)
    {
        ErrorOr<TransactionExportResult> result =
            await transactionService.ExportTransactionsAsync(model.ToExportRequest(format), ct);

        if (result.IsError)
        {
            TempData["ErrorMessage"] = result.FirstError.Description;
        }
        else
        {
            TempData["StatusMessage"] = $"Transactions exported to {result.Value.FileName}.";
            TempData["LastExportPath"] = result.Value.FileName;
        }

        return RedirectToAction(nameof(Index), ToRouteValues(model));
    }

    private static void PopulateFilterOptions(
        TransactionHistoryPageViewModel model,
        TransactionFilterMetadataResponse metadata)
    {
        model.AccountOptions = BuildOptions(
            "All Accounts",
            metadata.Accounts.Select(account => new TransactionFilterItemViewModel
            {
                Value = account.Id.ToString(CultureInfo.InvariantCulture),
                Label = string.IsNullOrWhiteSpace(account.Iban)
                    ? account.Name
                    : $"{account.Name} ({account.Iban})"
            }));

        model.CardOptions = BuildOptions(
            "All Cards",
            metadata.Cards.Select(card => new TransactionFilterItemViewModel
            {
                Value = card.Id.ToString(CultureInfo.InvariantCulture),
                Label = card.Label
            }));

        model.TransactionTypeOptions = BuildOptions("All Types", metadata.AvailableTransactionTypes);
        model.StatusOptions = BuildOptions("All Statuses", metadata.AvailableStatuses);
        model.DirectionOptions = BuildOptions("All Directions", metadata.AvailableDirections);
    }

    private static List<TransactionFilterItemViewModel> BuildOptions(
        string allLabel,
        IEnumerable<string> values) =>
        BuildOptions(
            allLabel,
            values.Select(v => new TransactionFilterItemViewModel { Value = v, Label = v }));

    private static List<TransactionFilterItemViewModel> BuildOptions(
        string allLabel,
        IEnumerable<TransactionFilterItemViewModel> values)
    {
        var options = new List<TransactionFilterItemViewModel>
        {
            new() { Value = string.Empty, Label = allLabel }
        };
        options.AddRange(values.Where(o => !string.IsNullOrWhiteSpace(o.Value)));
        return options;
    }

    private static object ToRouteValues(TransactionHistoryPageViewModel model) => new
    {
        model.SearchTerm,
        model.FromDate,
        model.ToDate,
        model.MinimumAmount,
        model.MaximumAmount,
        model.SelectedAccountId,
        model.SelectedCardId,
        model.SelectedTransactionType,
        model.SelectedStatus,
        model.SelectedDirection,
        model.SelectedSortField,
        model.SelectedSortDirection,
        model.SelectedTransactionId
    };

    private static int? ResolveSelectedTransactionId(
        List<TransactionHistoryItemDto> transactions,
        int? requestedId)
    {
        if (requestedId.HasValue && transactions.Any(t => t.Id == requestedId.Value))
        {
            return requestedId.Value;
        }

        return transactions.FirstOrDefault()?.Id;
    }
}