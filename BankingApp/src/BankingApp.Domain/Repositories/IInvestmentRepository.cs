namespace BankingApp.Domain.Repositories;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Domain.Aggregates.InvestmentAggregate;
using BankingApp.Domain.Aggregates.InvestmentAggregate.Entities;

/// <summary>Defines persistence operations for investment portfolios and trades.</summary>
public interface IInvestmentRepository
{
    /// <summary>Gets a user's portfolio snapshot including holdings.</summary>
    public Task<Portfolio?> GetPortfolioAsync(int userId, CancellationToken cancellationToken);

    /// <summary>Records a crypto buy or sell trade and updates holdings using final calculated values.</summary>
    public Task RecordCryptoTradeAsync(
        int portfolioId,
        string ticker,
        string actionType,
        decimal quantity,
        decimal pricePerUnit,
        decimal fees,
        decimal finalQuantity,
        decimal finalAveragePrice,
        CancellationToken cancellationToken);

    /// <summary>Gets investment transaction logs with optional filters.</summary>
    public Task<IReadOnlyCollection<InvestmentTransaction>> GetInvestmentLogsAsync(
        int portfolioId,
        DateTime? startDate,
        DateTime? endDate,
        string? ticker,
        CancellationToken cancellationToken);
}
