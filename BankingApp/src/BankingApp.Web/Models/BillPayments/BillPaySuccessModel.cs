namespace BankingApp.Web.Models.BillPayments;

/// <summary>
///     Model for the bill-payment success page (Success view).
/// </summary>
public class BillPaySuccessModel
{
    /// <summary>Gets or sets the unique receipt number for the completed payment.</summary>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the net payment amount (excluding fee).</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the fee charged for this payment.</summary>
    public decimal Fee { get; set; }

    /// <summary>Gets the total debited from the account (Amount + Fee).</summary>
    public decimal Total => Amount + Fee;

    /// <summary>Gets or sets the biller name.</summary>
    public string BillerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the currency code used for this payment.</summary>
    public string Currency { get; set; } = string.Empty;
}
