namespace BankingApp.Infrastructure.Persistence.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Domain.Aggregates.InvestmentAggregate;
using Domain.Aggregates.InvestmentAggregate.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class InvestmentRepository(AppDbContext dbContext) : IInvestmentRepository
{
    public async Task<Portfolio?> GetPortfolioAsync(int userId, CancellationToken cancellationToken)
    {
        Portfolio? portfolio =  await dbContext.Portfolios
            .Include(p => p.Holdings)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (portfolio == null)
        {
            portfolio = Portfolio.Create(userId);
            await dbContext.Portfolios.AddAsync(portfolio, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return portfolio;
    }

    public async Task RecordCryptoTradeAsync(
        int portfolioId,
        string ticker,
        string actionType,
        decimal quantity,
        decimal pricePerUnit,
        decimal fees,
        decimal finalQuantity,
        decimal finalAveragePrice,
        CancellationToken cancellationToken)
    {
        await using IDbContextTransaction tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            InvestmentHolding? holding = await dbContext.InvestmentHoldings
                .FirstOrDefaultAsync(h => h.PortfolioId == portfolioId && h.Ticker == ticker, cancellationToken);

            if (holding is not null)
            {
                holding.ApplyTrade(actionType, finalQuantity, finalAveragePrice, pricePerUnit, fees);
            }
            else
            {
                holding = InvestmentHolding.Create(portfolioId, ticker, "Crypto", finalQuantity, finalAveragePrice, pricePerUnit);
                dbContext.InvestmentHoldings.Add(holding);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var transaction = InvestmentTransaction.Create(
                holding.Id, ticker, actionType.ToUpperInvariant(), quantity, pricePerUnit, fees, "Market", DateTime.UtcNow);
            dbContext.InvestmentTransactions.Add(transaction);

            await dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<InvestmentTransaction>> GetInvestmentLogsAsync(
        int portfolioId,
        DateTime? startDate,
        DateTime? endDate,
        string? ticker,
        CancellationToken cancellationToken)
    {
        List<int> holdingIds = await dbContext.InvestmentHoldings
            .AsNoTracking()
            .Where(holding => holding.PortfolioId == portfolioId)
            .Select(holding => holding.Id)
            .ToListAsync(cancellationToken);

        IQueryable<InvestmentTransaction> query = dbContext.InvestmentTransactions
            .AsNoTracking()
            .Where(transaction => holdingIds.Contains(transaction.HoldingId));

        if (startDate.HasValue)
        {
            query = query.Where(transaction => transaction.ExecutedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(transaction => transaction.ExecutedAt <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(ticker))
        {
            query = query.Where(transaction => transaction.Ticker == ticker);
        }

        return await query
            .OrderByDescending(transaction => transaction.ExecutedAt)
            .ToListAsync(cancellationToken);
    }
}
