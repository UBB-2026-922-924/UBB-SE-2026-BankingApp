namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Represents a formatted transaction row for dashboard display.
/// </summary>
public class DashboardTransactionItem
{
    /// <summary>
    ///     Gets or sets the merchant or fallback display name.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string MerchantDisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the formatted amount string.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string AmountDisplay { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the currency code.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Currency { get; set; } = string.Empty;
}
