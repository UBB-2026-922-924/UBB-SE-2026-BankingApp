namespace BankingApp.Web.Models.Cards;

using BankingApp.Contracts.Features.Cards.Dtos;

/// <summary>Model for the cards list page (GET /Cards).</summary>
public class CardListModel
{
    /// <summary>Gets or sets the list of cards belonging to the current user.</summary>
    public List<CardDetailsDto> Cards { get; set; } = [];

    /// <summary>Gets a value indicating whether the user has any cards.</summary>
    public bool HasCards => Cards.Count > 0;

    /// <summary>
    /// Gets or sets the issue-card form model.
    /// Populated when the form is open or has a validation failure.
    /// </summary>
    public IssueCardModel IssueForm { get; set; } = new();

    /// <summary>Gets or sets a value indicating whether the issue-card form panel should be shown open.</summary>
    public bool ShowIssueForm { get; set; }
}
