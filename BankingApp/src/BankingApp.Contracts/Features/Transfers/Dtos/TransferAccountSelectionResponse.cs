namespace BankingApp.Contracts.Features.Transfers.Dtos;

/// <summary>
///     Represents an account option returned to the transfer UI.
/// </summary>
public class TransferAccountSelectionResponse
{
    /// <summary>Gets or sets the account identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the account IBAN.</summary>
    public string Iban { get; set; } = string.Empty;

    /// <summary>Gets or sets the account currency code.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets the current account balance.</summary>
    public decimal Balance { get; set; }

    /// <summary>Gets or sets the account display name.</summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>Gets the display name for the account.</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(AccountName)
        ? $"{Currency} Account ({(Iban.Length >= 4 ? "*" + Iban[^4..] : Iban)})"
        : AccountName;
}
