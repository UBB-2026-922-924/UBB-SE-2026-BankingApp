namespace BankingApp.Contracts.Features.BillPayments.Dtos;

/// <summary>
///     Represents the request payload for paying a bill.
/// </summary>
public class BillPayRequest
{
    /// <summary>
    ///     Gets or sets the source account identifier.
    /// </summary>
    /// <value>The source account identifier.</value>
    public int SourceAccountId { get; set; }

    /// <summary>
    ///     Gets or sets the biller identifier.
    /// </summary>
    /// <value>The biller identifier.</value>
    public int BillerId { get; set; }

    /// <summary>
    ///     Gets or sets the biller reference (account number, contract ID, etc.).
    /// </summary>
    /// <value>The biller reference.</value>
    public string BillerReference { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the payment amount.
    /// </summary>
    /// <value>The payment amount.</value>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to pay the full outstanding balance.
    /// </summary>
    /// <value>Whether to pay in full.</value>
    public bool IsPayInFull { get; set; }

}
