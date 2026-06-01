namespace BankingApp.Application.Features.Investments.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.InvestmentAggregate;
using Domain.Aggregates.InvestmentAggregate.Entities;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using Shared.Persistence;

public sealed class InvestmentsService(
    IInvestmentRepository investmentRepository,
    IUnitOfWork unitOfWork)
    : IInvestmentsService
{
    public async Task<ErrorOr<Portfolio>> GetPortfolioAsync(int userId, CancellationToken cancellationToken = default)
    {
        Portfolio? portfolio = await investmentRepository.GetPortfolioAsync(userId, cancellationToken);
        if (portfolio is null)
        {
            var created = Portfolio.Create(userId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return created;
        }

        return portfolio;
    }

    public async Task<ErrorOr<Success>> ExecuteTradeAsync(
        int userId,
        string ticker,
        string actionType,
        decimal quantity,
        decimal pricePerUnit,
        decimal fees,
        CancellationToken cancellationToken = default)
    {
        Portfolio? portfolio = await investmentRepository.GetPortfolioAsync(userId, cancellationToken);
        if (portfolio is null)
        {
            return InvestmentErrors.PortfolioNotFound;
        }

        decimal finalQuantity = actionType.Equals("BUY", StringComparison.OrdinalIgnoreCase)
            ? quantity
            : -quantity;

        InvestmentHolding? existing = portfolio.Holdings.FirstOrDefault(h => h.Ticker == ticker);
        decimal finalAveragePrice = existing is null
            ? pricePerUnit
            : existing.AvgPurchasePrice;

        ErrorOr<Success> tradeResult = portfolio.RecordTrade(
            ticker, "Crypto", actionType, quantity, pricePerUnit, fees, Math.Abs(finalQuantity), finalAveragePrice);

        if (tradeResult.IsError)
        {
            return tradeResult.FirstError;
        }

        await investmentRepository.RecordCryptoTradeAsync(
            portfolio.Id, ticker, actionType, quantity, pricePerUnit, fees,
            Math.Abs(finalQuantity), finalAveragePrice, cancellationToken);

        return Result.Success;
    }

    public async Task<ErrorOr<IReadOnlyCollection<InvestmentTransaction>>> GetLogsAsync(
        int userId,
        DateTime? from,
        DateTime? to,
        string? ticker,
        CancellationToken cancellationToken = default)
    {
        Portfolio? portfolio = await investmentRepository.GetPortfolioAsync(userId, cancellationToken);
        if (portfolio is null)
        {
            return InvestmentErrors.PortfolioNotFound;
        }

        IReadOnlyCollection<InvestmentTransaction> logs = await investmentRepository.GetInvestmentLogsAsync(
            portfolio.Id, from, to, ticker, cancellationToken);

        return ErrorOrFactory.From(logs);
    }
}
