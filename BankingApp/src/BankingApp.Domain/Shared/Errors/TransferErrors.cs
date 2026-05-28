namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

/// <summary>
///     Canonical error definitions for the transfer flow.
/// </summary>
public static class TransferErrors
{
    /// <summary>The recipient IBAN failed structural validation.</summary>
    public static readonly Error InvalidIban =
        Error.Validation("transfer.invalid_iban", "The recipient IBAN is invalid.");

    /// <summary>The transfer amount must be greater than zero.</summary>
    public static readonly Error InvalidAmount =
        Error.Validation("transfer.invalid_amount", "Transfer amount must be greater than zero.");

    /// <summary>The currency code must be exactly 3 characters.</summary>
    public static readonly Error InvalidCurrency =
        Error.Validation("transfer.invalid_currency", "Currency code must be exactly 3 characters.");

    /// <summary>The source account does not exist or does not belong to the user.</summary>
    public static readonly Error AccountNotFound =
        Error.NotFound("transfer.account_not_found", "Source account was not found.");

    /// <summary>The source account is not active (suspended or closed).</summary>
    public static readonly Error AccountNotActive =
        Error.Forbidden("transfer.account_not_active", "Source account is not active.");

    /// <summary>The source account has insufficient funds to cover the transfer and fee.</summary>
    public static readonly Error InsufficientFunds =
        Error.Forbidden("transfer.insufficient_funds", "Insufficient funds in the source account.");

    /// <summary>The transfer record could not be persisted.</summary>
    public static readonly Error PersistenceFailed =
        Error.Failure("transfer.persistence_failed", "Failed to save the transfer record.");

    /// <summary>The transaction log entry could not be persisted.</summary>
    public static readonly Error TransactionLogFailed =
        Error.Failure("transfer.transaction_log_failed", "Failed to log the transaction record.");

    /// <summary>The account debit operation failed.</summary>
    public static readonly Error DebitFailed =
        Error.Failure("transfer.debit_failed", "Failed to debit the source account.");

    public static readonly Error CurrencyMismatch =
        Error.Validation("transfer.currency_mismatch", "Transfer currency must match the source account currency.");

    public static readonly Error InvalidRecipientName =
        Error.Validation("transfer.invalid_recipient_name", "Recipient name is required.");

    public static readonly Error InvalidFee =
        Error.Validation("transfer.invalid_fee", "Transfer fee cannot be negative.");
}
