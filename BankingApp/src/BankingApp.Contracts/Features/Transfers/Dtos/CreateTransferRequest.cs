namespace BankingApp.Contracts.Features.Transfers.Dtos;

/// <summary>
///     Represents the data required to initiate a bank transfer.
/// </summary>
public class CreateTransferRequest
{
    /// <summary>Gets or sets the identifier of the source account to debit.</summary>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the recipient's full name.</summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient's IBAN.</summary>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the amount to transfer.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the ISO 4217 currency code (e.g. "RON", "EUR").</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional payment reference or description.</summary>
    public string? Reference { get; set; }

}
