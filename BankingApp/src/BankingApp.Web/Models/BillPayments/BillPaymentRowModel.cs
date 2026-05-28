namespace BankingApp.Web.Models.BillPayments;

/// <summary>
///     Represents a single row in the bill-payment history table.
/// </summary>
public class BillPaymentRowModel
{
    /// <summary>Gets or sets the payment id.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the unique receipt number.</summary>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the net payment amount (excluding fee).</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the fee charged for this payment.</summary>
    public decimal Fee { get; set; }

    /// <summary>Gets the total debited (Amount + Fee).</summary>
    public decimal Total => Amount + Fee;

    /// <summary>Gets or sets the payment status string (e.g. "Completed").</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp when the payment was created.</summary>
    public DateTime CreatedAt { get; set; }
}
