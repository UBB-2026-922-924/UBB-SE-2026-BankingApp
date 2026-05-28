namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

public static class ForexErrors
{
    public static readonly Error InvalidAmount =
        Error.Validation("forex.invalid_amount", "Forex amounts must be greater than zero.");

    public static readonly Error InvalidRate =
        Error.Validation("forex.invalid_rate", "The exchange rate produces a negative result.");

    public static readonly Error RateExpired =
        Error.Conflict("forex.rate_expired", "The locked exchange rate has expired.");

    public static readonly Error SameCurrency =
        Error.Validation("forex.same_currency", "Source and target currencies must be different.");

    public static readonly Error TransactionNotFound =
        Error.NotFound("forex.transaction_not_found", "Forex transaction was not found.");

    public static readonly Error InvalidCurrency =
        Error.Validation("forex.invalid_currency", "Invalid currency code.");

    public static readonly Error LockedRateMismatch =
        Error.Validation("forex.locked_rate_mismatch", "The locked rate does not match the requested currency pair.");

    public static readonly Error AccountCurrencyMismatch =
        Error.Validation("forex.account_currency_mismatch", "The selected accounts must match the requested currencies.");

    public static readonly Error InvalidCommission =
        Error.Validation("forex.invalid_commission", "The forex commission cannot be negative.");
}
