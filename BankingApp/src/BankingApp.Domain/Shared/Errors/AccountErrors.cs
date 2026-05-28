namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

public static class AccountErrors
{
    public static readonly Error NotFound =
        Error.NotFound("account.not_found", "Account was not found.");

    public static readonly Error NotActive =
        Error.Forbidden("account.not_active", "Account is not active.");

    public static readonly Error InsufficientFunds =
        Error.Forbidden("account.insufficient_funds", "Insufficient funds in the account.");

    public static readonly Error NegativeAmount =
        Error.Validation("account.negative_amount", "Amount cannot be negative.");

    public static readonly Error InvalidCurrency =
        Error.Validation("account.invalid_currency", "The specified currency is not supported.");

    public static readonly Error CurrencyMismatch =
        Error.Validation("account.currency_mismatch", "The money currency must match the account currency.");
}
