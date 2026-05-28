namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the operational status of a bank account.
/// </summary>
public enum AccountStatus
{
    /// <summary>The account is fully operational.</summary>
    Active,

    /// <summary>The account has been temporarily suspended.</summary>
    Suspended,

    /// <summary>The account has been permanently closed.</summary>
    Closed
}