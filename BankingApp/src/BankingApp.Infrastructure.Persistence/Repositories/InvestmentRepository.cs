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
        return await dbContext.Portfolios
            .Include(p => p.Holdings)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
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
        IQueryable<InvestmentTransaction> query = dbContext.InvestmentTransactions
            .AsNoTracking()
            .Where(t => t.Holding.PortfolioId == portfolioId);

        if (startDate.HasValue)
        {
            query = query.Where(t => t.ExecutedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.ExecutedAt <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(ticker))
        {
            query = query.Where(t => t.Ticker == ticker);
        }

        return await query
            .OrderByDescending(t => t.ExecutedAt)
            .ToListAsync(cancellationToken);
    }
}
