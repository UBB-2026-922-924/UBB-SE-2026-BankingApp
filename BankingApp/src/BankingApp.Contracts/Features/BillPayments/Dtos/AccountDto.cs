namespace BankingApp.Contracts.Features.BillPayments.Dtos;

/// <summary>
///     Represents a user account for bill payment source selection.
/// </summary>
public class AccountDto
{
    /// <summary>
    ///     Gets or sets the account identifier.
    /// </summary>
    /// <value>The account identifier.</value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the account IBAN.
    /// </summary>
    /// <value>The IBAN.</value>
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account currency.
    /// </summary>
    /// <value>The currency code.</value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account balance.
    /// </summary>
    /// <value>The account balance.</value>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Gets or sets the account display name.
    /// </summary>
    /// <value>The account name.</value>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the last four digits of the primary card linked to this account.
    ///     <c>null</c> when the account has no card.
    /// </summary>
    /// <value>The last four card digits, or <c>null</c>.</value>
    public string? CardLastFourDigits { get; set; }
}
