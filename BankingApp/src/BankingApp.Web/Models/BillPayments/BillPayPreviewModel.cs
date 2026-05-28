namespace BankingApp.Web.Models.BillPayments;

/// <summary>
///     Model for the bill-payment preview / confirmation step.
///     All fields are echoed as hidden inputs and carry the payment summary.
/// </summary>
public class BillPayPreviewModel
{
    /// <summary>Gets or sets the source account id.</summary>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the biller id.</summary>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the biller reference.</summary>
    public string BillerReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the payment amount.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the fee that will be charged.</summary>
    public decimal Fee { get; set; }

    /// <summary>Gets the total debited from the account (Amount + Fee).</summary>
    public decimal Total => Amount + Fee;

    /// <summary>Gets or sets the human-readable biller name for display.</summary>
    public string BillerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the account IBAN for display.</summary>
    public string AccountIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the account currency code for display.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets a server-side validation message to show on the preview page.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets a value indicating whether the biller should be saved after a successful payment.</summary>
    public bool ShouldSaveBiller { get; set; }
}
