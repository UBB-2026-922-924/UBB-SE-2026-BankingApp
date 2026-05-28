namespace BankingApp.Web.Models.Forex;

/// <summary>
///     Model for the forex rate preview / confirmation step.
///     All fields are echoed as hidden inputs to carry the exchange details forward.
/// </summary>
public class ForexPreviewModel
{
    /// <summary>Gets or sets the source account id.</summary>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the target account id.</summary>
    public int TargetAccountId { get; set; }

    /// <summary>Gets or sets the source currency code.</summary>
    public string FromCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the target currency code.</summary>
    public string ToCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the source amount to exchange.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the exchange rate applied.</summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>Gets or sets the converted amount in the target currency.</summary>
    public decimal ConvertedAmount { get; set; }

    /// <summary>Gets or sets the commission charged.</summary>
    public decimal Commission { get; set; }

    /// <summary>Gets or sets the source account IBAN for display.</summary>
    public string SourceAccountIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the target account IBAN for display.</summary>
    public string TargetAccountIban { get; set; } = string.Empty;

    /// <summary>Gets or sets a server-side error message to display on the preview page.</summary>
    public string? ErrorMessage { get; set; }
}
