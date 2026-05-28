namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Represents a single pre-formatted transfer row for display in the transfer history page.
/// </summary>
public class TransferHistoryDisplayItem
{
    /// <summary>
    ///     Gets or sets the recipient's full name.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the recipient's IBAN.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the recipient's bank name inferred from the IBAN.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string BankName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the formatted amount string (e.g. "-100.00").
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the currency (e.g. "EUR").
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the pre-formatted local date and time string.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string DateDisplay { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the human-readable transfer status (e.g. "Completed", "Pending").
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string StatusDisplay { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the transaction reference returned by the server, or "—" when none was assigned.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string ReferenceDisplay { get; set; } = string.Empty;
}
