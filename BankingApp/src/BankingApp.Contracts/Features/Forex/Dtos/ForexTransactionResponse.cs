namespace BankingApp.Contracts.Features.Forex.Dtos;

using Domain.Enums;

/// <summary>
///     Carries the result data of a currency exchange operation or rate preview.
/// </summary>
public class ForexTransactionResponse
{
    /// <summary>Gets or sets the unique identifier of the exchange (0 for previews).</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the ISO 4217 source currency code.</summary>
    /// <value>Gets or sets the current value.</value>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the ISO 4217 target currency code.</summary>
    /// <value>Gets or sets the current value.</value>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the amount credited to the target account.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal TargetAmount { get; set; }

    /// <summary>Gets or sets the applied exchange rate.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal ExchangeRate { get; set; }

    /// <summary>Gets or sets the commission charged for this exchange.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Commission { get; set; }

    /// <summary>Gets or sets the processing status.</summary>
    /// <value>Gets or sets the current value.</value>
    public ExchangeTransactionStatus Status { get; set; }
}
