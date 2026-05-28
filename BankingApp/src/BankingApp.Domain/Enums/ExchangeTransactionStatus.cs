namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the processing status of a foreign-exchange conversion transaction.
/// </summary>
public enum ExchangeTransactionStatus
{
    /// <summary>
    ///     The exchange has been submitted and is awaiting execution.
    /// </summary>
    Pending,

    /// <summary>
    ///     The exchange was executed and both accounts have been updated.
    /// </summary>
    Completed,

    /// <summary>
    ///     The exchange could not be completed due to a rate or balance issue.
    /// </summary>
    Failed,

    /// <summary>
    ///     The exchange was cancelled before execution.
    /// </summary>
    Cancelled
}