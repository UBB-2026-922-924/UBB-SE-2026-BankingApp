namespace BankingApp.Contracts.Features.BillPayments.Dtos;

/// <summary>
///     Represents the response returned after a successful bill payment.
/// </summary>
public class BillPayResponse
{
    /// <summary>
    ///     Gets or sets the payment identifier.
    /// </summary>
    /// <value>The payment identifier.</value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the receipt number.
    /// </summary>
    /// <value>The receipt number.</value>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the fee charged.
    /// </summary>
    /// <value>The fee amount.</value>
    public decimal Fee { get; set; }

    /// <summary>
    ///     Gets or sets the payment amount.
    /// </summary>
    /// <value>The payment amount.</value>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets the payment status.
    /// </summary>
    /// <value>The payment status.</value>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the UTC date and time at which the payment was created.
    /// </summary>
    /// <value>The creation timestamp.</value>
    public DateTime CreatedAt { get; set; }
}

