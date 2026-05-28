namespace BankingApp.Web.Models.BillPayments;

using System.ComponentModel.DataAnnotations;
using BankingApp.Contracts.Features.BillPayments.Dtos;
using BankingApp.Contracts.Features.Billers.Dtos;

/// <summary>
///     Model for the bill-payment entry form (Index).
/// </summary>
public class BillPayModel
{
    /// <summary>Gets or sets the list of the user's saved billers.</summary>
    public List<SavedBillerDto> SavedBillers { get; set; } = [];

    /// <summary>Gets or sets all active billers for the search dropdown.</summary>
    public List<BillerDto> AllBillers { get; set; } = [];

    /// <summary>Gets or sets the user's active accounts for the source-account dropdown.</summary>
    public List<AccountDto> Accounts { get; set; } = [];

    /// <summary>Gets or sets the selected biller id.</summary>
    [Required(ErrorMessage = "Please select a biller.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a biller.")]
    public int SelectedBillerId { get; set; }

    /// <summary>Gets or sets the selected source account id.</summary>
    [Required(ErrorMessage = "Please select an account.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select an account.")]
    public int SelectedAccountId { get; set; }

    /// <summary>Gets or sets the customer reference / invoice number at the biller.</summary>
    [Required(ErrorMessage = "Biller reference is required.")]
    [MaxLength(100, ErrorMessage = "Reference must be at most 100 characters.")]
    [Display(Name = "Biller Reference / Account Number")]
    public string BillerReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the payment amount.</summary>
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be between 0.01 and 1,000,000.")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>Gets or sets a value indicating whether the selected biller should be saved for future payments.</summary>
    public bool ShouldSaveBiller { get; set; }
}
