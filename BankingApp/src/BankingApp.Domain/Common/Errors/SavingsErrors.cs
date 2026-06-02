namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

public static class SavingsErrors
{
    public static Error AccountNotFound => Error.NotFound("Savings.AccountNotFound", "The savings account was not found.");
    public static Error InsufficientBalance => Error.Validation("Savings.InsufficientBalance", "Insufficient balance for this withdrawal.");
    public static Error AccountAlreadyClosed => Error.Conflict("Savings.AccountAlreadyClosed", "The savings account is already closed.");
    public static Error InvalidDepositAmount => Error.Validation("Savings.InvalidDepositAmount", "Deposit amount must be greater than zero.");
    public static Error AutoDepositNotFound => Error.NotFound("Savings.AutoDepositNotFound", "No auto-deposit configuration found for this account.");
}
