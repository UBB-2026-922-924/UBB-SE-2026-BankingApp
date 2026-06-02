namespace BankingApp.Domain.Aggregates.InvestmentAggregate.Entities;

using System;
using Common.Primitives;

/// <summary>Represents a single trade executed against an investment holding.</summary>
public sealed class InvestmentTransaction : Entity<int>
{
    private InvestmentTransaction()
    {
    }

    private InvestmentTransaction(int holdingId, string ticker, string actionType, decimal quantity, decimal pricePerUnit, decimal fees, string orderType, DateTime executedAt)
    {
        HoldingId = holdingId;
        Ticker = ticker;
        ActionType = actionType;
        Quantity = quantity;
        PricePerUnit = pricePerUnit;
        Fees = fees;
        OrderType = orderType;
        ExecutedAt = executedAt;
    }

    public int HoldingId { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public string ActionType { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal PricePerUnit { get; private set; }
    public decimal Fees { get; private set; }
    public string OrderType { get; private set; } = "Market";
    public DateTime ExecutedAt { get; private set; }

    public static InvestmentTransaction Create(int holdingId, string ticker, string actionType, decimal quantity, decimal pricePerUnit, decimal fees, string orderType, DateTime executedAt)
        => new(holdingId, ticker, actionType, quantity, pricePerUnit, fees, orderType, executedAt);
}
