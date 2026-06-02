namespace BankingApp.Contracts.Features.Investments.Dtos;

public sealed class ExecuteTradeRequest
{
    public string Ticker { get; init; } = string.Empty;

    public string ActionType { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public decimal PricePerUnit { get; init; }

    public decimal Fees { get; init; }
}
