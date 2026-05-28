namespace BankingApp.Web.Models.Forex;

/// <summary>
///     Represents a single row in the forex transaction history table.
/// </summary>
public class ForexHistoryRowModel
{
    /// <summary>Gets or sets the transaction id.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the source currency code.</summary>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the target currency code.</summary>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the converted target amount.</summary>
    public decimal TargetAmount { get; set; }

    /// <summary>Gets or sets the applied exchange rate.</summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>Gets or sets the commission charged.</summary>
    public decimal Commission { get; set; }

    /// <summary>Gets or sets the transaction status string.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets the Bootstrap badge CSS class for the current status.</summary>
    public string StatusBadgeClass => Status.ToLowerInvariant() switch
    {
        "completed" => "bg-success",
        "pending"   => "bg-warning text-dark",
        "failed"    => "bg-danger",
        "cancelled" => "bg-secondary",
        _           => "bg-secondary"
    };
}
