namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the type of payment card.
/// </summary>
public enum CardType
{
    /// <summary>
    ///     A debit card linked directly to a bank account.
    /// </summary>
    Debit,

    /// <summary>
    ///     A credit card backed by a line of credit.
    /// </summary>
    Credit,

    /// <summary>
    ///     A prepaid card loaded with a fixed amount.
    /// </summary>
    Prepaid
}