namespace BankingApp.Web.Models.Transfers;

using System.ComponentModel.DataAnnotations;
using BankingApp.Contracts.Features.Transfers.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
///     View model for step 2: IBAN validated, select source account + amount.
/// </summary>
public class TransferIbanValidatedModel
{
    /// <summary>Gets or sets the validated recipient IBAN.</summary>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the bank name inferred from the IBAN.</summary>
    public string RecipientBankName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's accounts for the dropdown.</summary>
    public List<TransferAccountSelectionResponse> Accounts { get; set; } = [];

    /// <summary>Gets a <see cref="SelectList"/> built from <see cref="Accounts"/>.</summary>
    public SelectList AccountSelectList =>
        new(Accounts.Select(a => new
            {
                a.Id,
                Display = $"{a.AccountName} — {a.Iban} ({a.Currency}) | Balance: {a.Balance:N2}"
            }),
            "Id", "Display", SelectedAccountId);

    /// <summary>Gets or sets the selected source account id.</summary>
    [Required(ErrorMessage = "Please select a source account.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a source account.")]
    [Display(Name = "From Account")]
    public int SelectedAccountId { get; set; }

    /// <summary>Gets or sets the recipient name.</summary>
    [Required(ErrorMessage = "Recipient name is required.")]
    [MaxLength(100, ErrorMessage = "Recipient name must be 100 characters or fewer.")]
    [Display(Name = "Recipient Name")]
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the transfer amount.</summary>
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be between 0.01 and 1,000,000.")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the currency code.</summary>
    [Required(ErrorMessage = "Currency is required.")]
    [Display(Name = "Currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional payment reference.</summary>
    [MaxLength(200, ErrorMessage = "Reference must be 200 characters or fewer.")]
    [Display(Name = "Reference (optional)")]
    public string? Reference { get; set; }

    /// <summary>Gets the list of supported currencies.</summary>
    public static IReadOnlyList<string> SupportedCurrencies { get; } =
        ["USD", "EUR", "GBP", "RON", "CHF", "JPY", "CAD", "AUD"];
}
