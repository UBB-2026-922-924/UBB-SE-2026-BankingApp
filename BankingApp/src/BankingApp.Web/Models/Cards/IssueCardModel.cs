namespace BankingApp.Web.Models.Cards;

using System.ComponentModel.DataAnnotations;
using Domain.Enums;

/// <summary>Form model for issuing a new card.</summary>
public class IssueCardModel
{
    /// <summary>Gets or sets the card type (Debit or Credit).</summary>
    [Required(ErrorMessage = "Please select a card type.")]
    public CardType CardType { get; set; }

    /// <summary>Gets or sets the card brand (Visa or Mastercard).</summary>
    [Required(ErrorMessage = "Please select a card brand.")]
    public string CardBrand { get; set; } = string.Empty;
}
