namespace BankingApp.Web.Models.Transfers;

/// <summary>
///     Represents a single row in the transfer history table.
/// </summary>
public class TransferHistoryRowModel
{
    /// <summary>Gets or sets the transfer id.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the recipient IBAN.</summary>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient name.</summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the transfer amount.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the currency code.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets the transfer status string.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the transfer reference.</summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>Gets or sets the creation date.</summary>
    public DateTime CreatedAt { get; set; }

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
