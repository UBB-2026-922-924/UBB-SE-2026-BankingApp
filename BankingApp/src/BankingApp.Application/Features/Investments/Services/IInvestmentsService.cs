namespace BankingApp.Application.Features.Investments.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.InvestmentAggregate;
using Domain.Aggregates.InvestmentAggregate.Entities;
using ErrorOr;

public interface IInvestmentsService
{
    public Task<ErrorOr<Portfolio>> GetPortfolioAsync(int userId, CancellationToken cancellationToken = default);

    public Task<ErrorOr<Success>> ExecuteTradeAsync(
        int userId,
        string ticker,
        string actionType,
        decimal quantity,
        decimal pricePerUnit,
        decimal fees,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<IReadOnlyCollection<InvestmentTransaction>>> GetLogsAsync(
        int userId,
        DateTime? from,
        DateTime? to,
        string? ticker,
        CancellationToken cancellationToken = default);
}
