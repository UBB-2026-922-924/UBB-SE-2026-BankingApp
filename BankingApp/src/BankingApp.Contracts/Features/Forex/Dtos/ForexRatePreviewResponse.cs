namespace BankingApp.Contracts.Features.Forex.Dtos;

public class ForexRatePreviewResponse
{
    public string SourceCurrency { get; set; } = string.Empty;
    public string TargetCurrency { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Commission { get; set; }
}
