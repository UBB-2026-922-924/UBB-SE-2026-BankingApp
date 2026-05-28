namespace BankingApp.Contracts.Features.BillPayments.Dtos;

/// <summary>
/// Data transfer object used for initiating a new bill payment request.
/// </summary>
public class BillPaymentDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user initiating the payment.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the account from which to draw funds.
    /// </summary>
    public int SourceAccountId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the biller receiving the payment.
    /// </summary>
    public int BillerId { get; set; }

    /// <summary>
    /// Gets or sets the customer reference or invoice number.
    /// </summary>
    public required string BillerReference { get; set; }

    /// <summary>
    /// Gets or sets the amount to be paid.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the bill is being paid in full.
    /// </summary>
    public bool IsPayInFull { get; set; }
}
