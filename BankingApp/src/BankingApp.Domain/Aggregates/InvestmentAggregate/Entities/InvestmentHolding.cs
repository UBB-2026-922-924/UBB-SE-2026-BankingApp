namespace BankingApp.Domain.Aggregates.InvestmentAggregate.Entities;

using System.Collections.Generic;
using Common.Primitives;

/// <summary>Represents a specific asset holding within a user's investment portfolio.</summary>
public sealed class InvestmentHolding : Entity<int>
{
    private readonly List<InvestmentTransaction> _transactions = [];

    private InvestmentHolding()
    {
    }

    private InvestmentHolding(int portfolioId, string ticker, string assetType, decimal quantity, decimal avgPurchasePrice, decimal currentPrice)
    {
        PortfolioId = portfolioId;
        Ticker = ticker;
        AssetType = assetType;
        Quantity = quantity;
        AvgPurchasePrice = avgPurchasePrice;
        CurrentPrice = currentPrice;
        UnrealizedGainLoss = (currentPrice - avgPurchasePrice) * quantity;
    }

    public int PortfolioId { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public string AssetType { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal AvgPurchasePrice { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public decimal UnrealizedGainLoss { get; private set; }

    public IReadOnlyCollection<InvestmentTransaction> Transactions => _transactions.AsReadOnly();

    public static InvestmentHolding Create(int portfolioId, string ticker, string assetType, decimal quantity, decimal avgPurchasePrice, decimal currentPrice)
        => new(portfolioId, ticker, assetType, quantity, avgPurchasePrice, currentPrice);

    /// <summary>Updates quantity, average price, and unrealized gain/loss after a trade is applied.</summary>
    public void ApplyTrade(string actionType, decimal finalQuantity, decimal finalAveragePrice, decimal currentPrice, decimal fees)
    {
        Quantity = finalQuantity;
        AvgPurchasePrice = finalAveragePrice;
        CurrentPrice = currentPrice;
        UnrealizedGainLoss = (CurrentPrice - AvgPurchasePrice) * Quantity;
    }

    /// <summary>Appends a transaction record to this holding's history.</summary>
    public void AddTransaction(InvestmentTransaction transaction)
    {
        _transactions.Add(transaction);
    }
}
