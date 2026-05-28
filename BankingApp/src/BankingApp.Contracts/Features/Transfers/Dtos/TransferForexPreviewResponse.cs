namespace BankingApp.Contracts.Features.Transfers.Dtos;

/// <summary>
///     Represents the FX preview for a transfer.
/// </summary>
public class TransferForexPreviewResponse
{
    /// <summary>Gets or sets the exchange rate.</summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>Gets or sets the converted amount.</summary>
    public decimal ConvertedAmount { get; set; }
}
