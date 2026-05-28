namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the type of bank account.
/// </summary>
public enum AccountType
{
    /// <summary>A standard checking account for everyday transactions.</summary>
    Checking,

    /// <summary>A savings account intended for accumulating funds.</summary>
    Savings,

    /// <summary>A business-purpose account.</summary>
    Business,

    /// <summary>A credit account.</summary>
    Credit
}