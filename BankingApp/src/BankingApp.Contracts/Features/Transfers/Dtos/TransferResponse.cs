namespace BankingApp.Contracts.Features.Transfers.Dtos;

using Domain.Enums;

/// <summary>
///     Represents a transfer record returned to the caller.
/// </summary>
public class TransferResponse
{
    /// <summary>Gets or sets the transfer identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the source account.</summary>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the identifier of the logged transaction.</summary>
    public int? TransactionId { get; set; }

    /// <summary>Gets or sets the logged transaction reference when available.</summary>
    public string? TransactionRef { get; set; }

    /// <summary>Gets or sets the recipient's full name.</summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient's IBAN.</summary>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient's bank name inferred from the IBAN.</summary>
    public string? RecipientBankName { get; set; }

    /// <summary>Gets or sets the transfer amount.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the currency code.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets the fee applied to this transfer.</summary>
    public decimal Fee { get; set; }

    /// <summary>Gets or sets the optional payment reference.</summary>
    public string? Reference { get; set; }

    /// <summary>Gets or sets the transfer status.</summary>
    public TransferStatus Status { get; set; }

    /// <summary>Gets or sets the UTC date and time when the transfer was created.</summary>
    public DateTime CreatedAt { get; set; }
}
