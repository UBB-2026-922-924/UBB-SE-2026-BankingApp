namespace BankingApp.Web.Controllers;

using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Features.Beneficiaries.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Beneficiaries;

[Authorize]
public class BeneficiariesController(IBeneficiaryService beneficiaryService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ErrorOr<List<BeneficiaryDto>> result = await beneficiaryService.GetAllAsync(cancellationToken);
        if (result.IsError)
        {
            TempData["Error"] = "Could not load beneficiaries.";
            return View(new BeneficiaryListModel());
        }

        return View(BeneficiaryListModel.FromBeneficiaries(result.Value));
    }

    public IActionResult Create() => View(new BeneficiaryFormModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BeneficiaryFormModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ErrorOr<Success> createResult = await beneficiaryService.CreateAsync(model.ToCreateRequest(), cancellationToken);
        if (createResult.IsError)
        {
            ModelState.AddModelError(string.Empty, "Could not save beneficiary. Check that the IBAN is valid and not already saved.");
            return View(model);
        }

        TempData["Success"] = $"{model.Name.Trim()} was added to your beneficiaries.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ErrorOr<BeneficiaryDto> result = await beneficiaryService.GetByIdAsync(id, cancellationToken);
        if (result.IsError)
        {
            return NotFound();
        }

        return View(BeneficiaryFormModel.FromBeneficiary(result.Value));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BeneficiaryFormModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ErrorOr<Success> updateResult = await beneficiaryService.UpdateAsync(id, model.ToUpdateRequest(), cancellationToken);
        if (updateResult.IsError)
        {
            ModelState.AddModelError(string.Empty, "Could not update beneficiary. The IBAN may be invalid.");
            return View(model);
        }

        TempData["Success"] = $"{model.Name.Trim()} was updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await beneficiaryService.DeleteAsync(id, cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? "Could not remove beneficiary."
            : "Beneficiary removed.";

        return RedirectToAction(nameof(Index));
    }
}
