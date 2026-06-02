namespace BankingApp.Domain.Aggregates.InvestmentAggregate;

using System.Collections.Generic;
using System.Linq;
using BankingApp.Domain.Aggregates.InvestmentAggregate.Entities;
using BankingApp.Domain.Common.Errors;
using BankingApp.Domain.Common.Primitives;
using ErrorOr;

public sealed class Portfolio : AggregateRoot<int>
{
    private readonly List<InvestmentHolding> _holdings = [];

    private Portfolio()
    {
    }

    private Portfolio(int userId)
    {
        UserId = userId;
    }

    public int UserId { get; private set; }

    public IReadOnlyCollection<InvestmentHolding> Holdings => _holdings.AsReadOnly();

    public decimal TotalValue => _holdings.Sum(h => h.Quantity * h.CurrentPrice);
    public decimal TotalCostBasis => _holdings.Sum(h => h.Quantity * h.AvgPurchasePrice);
    public decimal TotalGainLoss => TotalValue - TotalCostBasis;
    public decimal GainLossPercent => TotalCostBasis == 0 ? 0 : TotalGainLoss / TotalCostBasis;

    public static Portfolio Create(int userId) => new(userId);

    /// <summary>Records a buy or sell trade, updating or creating the matching holding.</summary>
    public ErrorOr<Success> RecordTrade(
        string ticker,
        string assetType,
        string actionType,
        decimal quantity,
        decimal pricePerUnit,
        decimal fees,
        decimal finalQuantity,
        decimal finalAveragePrice)
    {
        if (quantity <= 0)
        {
            return InvestmentErrors.InvalidTradeQuantity;
        }

        InvestmentHolding? holding = _holdings.FirstOrDefault(h => h.Ticker == ticker);
        if (holding is null)
        {
            holding = InvestmentHolding.Create(Id, ticker, assetType, finalQuantity, finalAveragePrice, pricePerUnit);
            _holdings.Add(holding);
        }
        else
        {
            holding.ApplyTrade(actionType, finalQuantity, finalAveragePrice, pricePerUnit, fees);
        }

        var transaction = InvestmentTransaction.Create(holding.Id, ticker, actionType, quantity, pricePerUnit, fees, "Market", System.DateTime.UtcNow);
        holding.AddTransaction(transaction);

        return Result.Success;
    }
}
