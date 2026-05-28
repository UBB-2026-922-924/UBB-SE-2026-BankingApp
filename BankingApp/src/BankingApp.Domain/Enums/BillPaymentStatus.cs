namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the processing status of a bill payment.
/// </summary>
public enum BillPaymentStatus
{
    /// <summary>
    ///     The payment has been submitted and is awaiting processing.
    /// </summary>
    Pending,

    /// <summary>
    ///     The payment was processed and the bill was settled.
    /// </summary>
    Completed,

    /// <summary>
    ///     The payment failed due to insufficient funds or a biller error.
    /// </summary>
    Failed
}