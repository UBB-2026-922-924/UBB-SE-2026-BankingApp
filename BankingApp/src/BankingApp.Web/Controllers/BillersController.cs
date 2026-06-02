namespace BankingApp.Web.Controllers;

using Contracts.Features.Billers.Dtos;
using Contracts.Features.Billers.Services;
using ViewModels;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class BillersController(IBillerService billerService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ErrorOr<List<BillerDto>> billersResult = await billerService.GetBillersAsync(ct: cancellationToken);
        ErrorOr<List<SavedBillerDto>> savedResult = await billerService.GetSavedBillersAsync(cancellationToken);

        if (billersResult.IsError)
        {
            TempData["Error"] = "Could not load billers.";
            return View(new BillerListViewModel());
        }

        List<int> savedIds = savedResult.IsError
            ? []
            : savedResult.Value.ConvertAll(s => s.BillerId);

        IList<BillerListViewModel.BillerRow> rows = billersResult.Value
            .ConvertAll(b => new BillerListViewModel.BillerRow
            {
                Id = b.Id,
                Name = b.Name,
                Category = b.Category,
                LogoUrl = b.LogoUrl,
                IsSaved = savedIds.Contains(b.Id)
            });

        return View(new BillerListViewModel { Billers = rows });
    }

    [HttpGet]
    public async Task<IActionResult> Saved(CancellationToken cancellationToken)
    {
        ErrorOr<List<SavedBillerDto>> result = await billerService.GetSavedBillersAsync(cancellationToken);
        if (result.IsError)
        {
            TempData["Error"] = "Could not load saved billers.";
            return View(new SavedBillerListViewModel());
        }

        IList<SavedBillerListViewModel.SavedBillerRow> rows = result.Value
            .ConvertAll(s => new SavedBillerListViewModel.SavedBillerRow
            {
                Id = s.Id,
                BillerId = s.BillerId,
                DisplayName = s.DisplayName,
                Category = s.DisplayCategory,
                LogoUrl = s.LogoUrl,
                DefaultReference = s.DefaultReference
            });

        return View(new SavedBillerListViewModel { SavedBillers = rows });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Billers/Save/{billerId:int}")]
    public async Task<IActionResult> Save(int billerId, CancellationToken cancellationToken)
    {
        ErrorOr<SavedBillerDto> result = await billerService.SaveBillerAsync(
            new SaveBillerRequest { BillerId = billerId },
            cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? "Could not save biller."
            : "Biller saved.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Billers/Unsave/{savedBillerId:int}")]
    public async Task<IActionResult> Unsave(int savedBillerId, CancellationToken cancellationToken)
    {
        TempData["Error"] = "Unsave is not yet supported.";
        return RedirectToAction(nameof(Saved));
    }
}