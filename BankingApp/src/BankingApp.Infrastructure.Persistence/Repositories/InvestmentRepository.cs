namespace BankingApp.Infrastructure.Persistence.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Aggregates.InvestmentAggregate;
using Domain.Repositories;
using Data;
using Domain.Aggregates.InvestmentAggregate.Entities;
using Microsoft.EntityFrameworkCore.Storage;

public class InvestmentRepository(AppDbContext db) : IInvestmentRepository
{
    public Portfolio GetPortfolio(int userId)
    {
        // We search for the portfolio
        var portfolio = db.Portfolios
            .Include(p => p.Holdings)
            .FirstOrDefault(p => p.UserId == userId);

        // If it doesn't exist, we create AND SAVE it immediately.
        if (portfolio == null)
        {
            portfolio = Portfolio.Create(userId);
            db.Portfolios.Add(portfolio);
            db.SaveChanges();
        }

        return portfolio;
    }

    public async Task<List<InvestmentTransaction>> GetInvestmentLogsAsync(int portfolioId, DateTime? startDate, DateTime? endDate, string? ticker)
    {
        // Use _db.InvestmentTransactions explicitly to ensure the type is known
        var query = db.InvestmentTransactions
            .AsNoTracking()
            .Include(x => x.Holding)
            .Where(x => x.Holding.PortfolioId == portfolioId);

        if (startDate.HasValue)
        {
            query = query.Where(x => x.ExecutedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.ExecutedAt <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(ticker))
        {
            query = query.Where(x => x.Ticker == ticker);
        }

        return await query.OrderByDescending(x => x.ExecutedAt).ToListAsync();
    }

    public async Task RecordCryptoTradeAsync(int portfolioId, string ticker, string actionType, decimal quantity,
        decimal pricePerUnit, decimal fees, decimal finalQuantity, decimal finalAveragePrice)
    {
        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var holding = await db.InvestmentHoldings
                .FirstOrDefaultAsync(h => h.PortfolioId == portfolioId && h.Ticker == ticker);

            if (holding != null)
            {
                holding.Quantity = finalQuantity;
                holding.AvgPurchasePrice = finalAveragePrice;
                holding.CurrentPrice = pricePerUnit;
            }
            else
            {
                holding = new InvestmentHolding
                {
                    PortfolioId = portfolioId,
                    Ticker = ticker,
                    AssetType = "Crypto",
                    Quantity = finalQuantity,
                    AvgPurchasePrice = finalAveragePrice,
                    CurrentPrice = pricePerUnit
                };
                db.InvestmentHoldings.Add(holding);
                await db.SaveChangesAsync();
            }

            db.InvestmentTransactions.Add(new InvestmentTransaction
            {
                HoldingId = holding.Id,
                Ticker = ticker,
                ActionType = actionType.ToUpperInvariant(),
                Quantity = quantity,
                PricePerUnit = pricePerUnit,
                Fees = fees,
                ExecutedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}