namespace BankingApp.Web.Controllers;

using BankingApp.Contracts.Features.Cards.Dtos;
using BankingApp.Contracts.Features.Cards.Services;
using BankingApp.Domain.Enums;
using BankingApp.Web.Models.Cards;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>Handles card listing, issuing, freezing, unfreezing and cancellation.</summary>
[Authorize]
public class CardsController(ICardService cardService) : Controller
{
    /// <summary>Displays the list of cards for the current user.</summary>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ErrorOr<List<CardDetailsDto>> result = await cardService.GetCardsAsync(cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Could not load cards. Please try again.";
            return View(new CardListModel());
        }

        return View(new CardListModel { Cards = result.Value });
    }

    /// <summary>Processes the issue-card form submission.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Issue(IssueCardModel issueForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnIndexWithIssueFormAsync(issueForm, cancellationToken);
        }

        IssueCardRequest request = new()
        {
            CardType = issueForm.CardType,
            CardBrand = issueForm.CardBrand,
        };

        ErrorOr<CardDetailsDto> result = await cardService.IssueCardAsync(request, cancellationToken);

        if (result.IsError)
        {
            ModelState.AddModelError(string.Empty, "Could not issue card. Please try again.");
            return await ReturnIndexWithIssueFormAsync(issueForm, cancellationToken);
        }

        TempData["Success"] = $"Your new {issueForm.CardBrand} {issueForm.CardType} card has been issued successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Freezes the specified card.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Freeze(int id, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await cardService.FreezeCardAsync(id, cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? "Could not freeze the card. Please try again."
            : "Card has been frozen successfully.";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Unfreezes the specified card.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unfreeze(int id, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await cardService.UnfreezeCardAsync(id, cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? "Could not unfreeze the card. Please try again."
            : "Card has been unfrozen successfully.";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Cancels the specified card.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await cardService.CancelCardAsync(id, cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? "Could not cancel the card. Please try again."
            : "Card has been cancelled.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnIndexWithIssueFormAsync(
        IssueCardModel issueForm,
        CancellationToken cancellationToken)
    {
        ErrorOr<List<CardDetailsDto>> cardsResult = await cardService.GetCardsAsync(cancellationToken);

        CardListModel listModel = new()
        {
            Cards = cardsResult.IsError ? [] : cardsResult.Value,
            IssueForm = issueForm,
            ShowIssueForm = true,
        };

        return View(nameof(Index), listModel);
    }
}
