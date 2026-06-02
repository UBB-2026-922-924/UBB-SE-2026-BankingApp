namespace BankingApp.Web.Models.Forex;

using System.ComponentModel.DataAnnotations;
using BankingApp.Contracts.Features.BillPayments.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
///     Model for the forex exchange entry form (GET /Forex).
/// </summary>
public class ForexExchangeModel
{
    /// <summary>Gets or sets the user's active accounts for the source-account dropdown.</summary>
    public List<AccountDto> Accounts { get; set; } = [];

    /// <summary>Gets a <see cref="SelectList"/> built from <see cref="Accounts"/>.</summary>
    public SelectList AccountSelectList =>
        new(Accounts.Select(a => new
            {
                a.Id,
                Display = $"{a.AccountName} — {a.Iban} ({a.Currency}) | Balance: {a.Balance:N2}"
            }),
            "Id", "Display", SelectedSourceAccountId);

    /// <summary>Gets or sets the selected source account id.</summary>
    [Required(ErrorMessage = "Please select a source account.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a source account.")]
    [Display(Name = "From Account")]
    public int SelectedSourceAccountId { get; set; }

    /// <summary>Gets or sets the selected target account id.</summary>
    [Required(ErrorMessage = "Please select a target account.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a target account.")]
    [Display(Name = "To Account")]
    public int SelectedTargetAccountId { get; set; }

    /// <summary>Gets or sets the source currency code.</summary>
    [Required(ErrorMessage = "Source currency is required.")]
    [Display(Name = "From Currency")]
    public string FromCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the target currency code.</summary>
    [Required(ErrorMessage = "Target currency is required.")]
    [Display(Name = "To Currency")]
    public string ToCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the amount to exchange.</summary>
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be between 0.01 and 1,000,000.")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }
}
