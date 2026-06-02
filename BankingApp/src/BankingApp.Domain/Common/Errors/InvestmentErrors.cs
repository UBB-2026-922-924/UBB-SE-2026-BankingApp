namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

public static class InvestmentErrors
{
    public static Error PortfolioNotFound => Error.NotFound("Investment.PortfolioNotFound", "The investment portfolio was not found.");
    public static Error HoldingNotFound => Error.NotFound("Investment.HoldingNotFound", "The investment holding was not found.");
    public static Error InsufficientFunds => Error.Validation("Investment.InsufficientFunds", "Insufficient funds to complete this trade.");
    public static Error InvalidTradeQuantity => Error.Validation("Investment.InvalidTradeQuantity", "Trade quantity must be greater than zero.");
}
