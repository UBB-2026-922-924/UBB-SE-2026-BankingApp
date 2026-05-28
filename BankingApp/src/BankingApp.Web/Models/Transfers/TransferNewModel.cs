namespace BankingApp.Web.Models.Transfers;

using System.ComponentModel.DataAnnotations;

/// <summary>
///     View model for step 1: enter recipient IBAN (GET /Transfers/New).
/// </summary>
public class TransferNewModel
{
    /// <summary>Gets or sets the recipient IBAN.</summary>
    [Required(ErrorMessage = "Recipient IBAN is required.")]
    [MaxLength(34, ErrorMessage = "IBAN must be 34 characters or fewer.")]
    [Display(Name = "Recipient IBAN")]
    public string RecipientIban { get; set; } = string.Empty;
}
