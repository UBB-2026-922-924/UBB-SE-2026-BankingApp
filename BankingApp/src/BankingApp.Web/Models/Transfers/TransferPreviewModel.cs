namespace BankingApp.Web.Models.Transfers;

using Contracts.Features.Transfers;

/// <summary>
///     View model for the transfer preview / confirmation step.
///     Shows forex preview (if cross-currency) and transfer summary.
/// </summary>
public class TransferPreviewModel
{
    /// <summary>Gets or sets the source account id.</summary>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the source account IBAN.</summary>
    public string SourceAccountIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the source account currency.</summary>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient name.</summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient IBAN.</summary>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient bank name.</summary>
    public string RecipientBankName { get; set; } = string.Empty;

    /// <summary>Gets or sets the transfer amount.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets the fixed transfer fee charged in the transfer currency.</summary>
    public decimal Fee => TransferPricing.Fee;

    /// <summary>Gets the total amount debited from the source account.</summary>
    public decimal TotalDebit => Amount + Fee;

    /// <summary>Gets or sets the transfer currency.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional reference.</summary>
    public string? Reference { get; set; }

    /// <summary>Gets or sets whether this is a cross-currency transfer.</summary>
    public bool IsCrossCurrency { get; set; }

    /// <summary>Gets or sets the forex exchange rate (if cross-currency).</summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>Gets or sets the converted amount (if cross-currency).</summary>
    public decimal? ConvertedAmount { get; set; }

    /// <summary>Gets or sets a server-side error message.</summary>
    public string? ErrorMessage { get; set; }
}
