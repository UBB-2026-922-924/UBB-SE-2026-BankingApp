namespace BankingApp.Application.Features.Forex.Services;

using Contracts.Features.Forex.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Aggregates.ForexAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;
using Currency = NodaMoney.Currency;
using Money = NodaMoney.Money;

public sealed class ForexService(
    IAccountRepository accountRepository,
    IForexRepository forexRepository,
    ILockedRateCache lockedRateCache,
    IExchangeRateService exchangeRateService,
    IUnitOfWork unitOfWork,
    ISystemClock clock)
    : IForexService
{
    private static readonly TimeSpan _rateTtl = TimeSpan.FromSeconds(300);

    public Task<ErrorOr<ForexRatePreviewResponse>> GetRatePreviewAsync(
        int userId, string sourceCurrency, string targetCurrency, decimal sourceAmount, CancellationToken cancellationToken = default)
    {
        Currency source;
        Currency target;
        try
        {
            source = Currency.FromCode(sourceCurrency);
            target = Currency.FromCode(targetCurrency);
        }
        catch
        {
            return Task.FromResult<ErrorOr<ForexRatePreviewResponse>>(ForexErrors.InvalidCurrency);
        }

        if (source == target)
        {
            return Task.FromResult<ErrorOr<ForexRatePreviewResponse>>(ForexErrors.SameCurrency);
        }

        ErrorOr<decimal> rateResult = exchangeRateService.GetRate(source, target);
        if (rateResult.IsError)
        {
            return Task.FromResult<ErrorOr<ForexRatePreviewResponse>>(rateResult.FirstError);
        }

        decimal rate = rateResult.Value;
        lockedRateCache.Store(userId, source, target, rate, clock.UtcNow);

        return Task.FromResult<ErrorOr<ForexRatePreviewResponse>>(new ForexRatePreviewResponse
        {
            SourceCurrency = source.Code,
            TargetCurrency = target.Code,
            TargetAmount = Math.Round(sourceAmount * rate, 2),
            ExchangeRate = rate,
            Commission = Math.Round(sourceAmount * ForexPolicy.CommissionRate, 2)
        });
    }

    public async Task<ErrorOr<ForexTransactionResponse>> ExecuteAsync(
        int userId, int sourceAccountId, int targetAccountId,
        string sourceCurrency, string targetCurrency, decimal sourceAmount, CancellationToken cancellationToken = default)
    {
        Currency srcCurrency;
        Currency tgtCurrency;
        try
        {
            srcCurrency = Currency.FromCode(sourceCurrency);
            tgtCurrency = Currency.FromCode(targetCurrency);

            if (srcCurrency == tgtCurrency)
            {
                return ForexErrors.SameCurrency;
            }
        }
        catch
        {
            return ForexErrors.InvalidCurrency;
        }

        LockedRate? lockedRate = lockedRateCache.TryGet(userId);
        if (lockedRate is null || clock.UtcNow - lockedRate.LockedAt > _rateTtl)
        {
            return ForexErrors.RateExpired;
        }

        if (lockedRate.BaseCurrency != srcCurrency || lockedRate.QuoteCurrency != tgtCurrency)
        {
            return ForexErrors.LockedRateMismatch;
        }

        Account? sourceAccount = await accountRepository.GetByIdAsync(sourceAccountId, cancellationToken);
        if (sourceAccount is null || sourceAccount.UserId != userId)
        {
            return AccountErrors.NotFound;
        }

        Account? targetAccount = await accountRepository.GetByIdAsync(targetAccountId, cancellationToken);
        if (targetAccount is null || targetAccount.UserId != userId)
        {
            return AccountErrors.NotFound;
        }

        if (!sourceAccount.IsActive() || !targetAccount.IsActive())
        {
            return AccountErrors.NotActive;
        }

        if (!sourceAccount.UsesCurrency(srcCurrency) || !targetAccount.UsesCurrency(tgtCurrency))
        {
            return ForexErrors.AccountCurrencyMismatch;
        }

        decimal rate = lockedRate.Rate;
        DateTime now = clock.UtcNow;
        Money srcAmount = new(sourceAmount, srcCurrency);
        Money commission = new(Math.Round(sourceAmount * ForexPolicy.CommissionRate, 2), srcCurrency);
        Money tgtAmount = new(Math.Round(sourceAmount * rate, 2), tgtCurrency);

        ErrorOr<ForexTransaction> forexResult = ForexTransaction.Create(
            userId, sourceAccountId, targetAccountId, srcAmount, tgtAmount, rate, commission, now);

        if (forexResult.IsError)
        {
            return forexResult.FirstError;
        }

        string forexRef = $"FX-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

        ErrorOr<Money> sourceBalanceResult = sourceAccount.Debit(srcAmount + commission, now);
        if (sourceBalanceResult.IsError)
        {
            return sourceBalanceResult.FirstError;
        }

        Transaction sourceTxn = sourceAccount.RecordTransaction(
            forexRef, "FOREX", TransactionDirection.Out, srcAmount,
            sourceBalanceResult.Value, TransactionStatus.Completed, now);

        ErrorOr<Money> targetBalanceResult = targetAccount.Credit(tgtAmount, now);
        if (targetBalanceResult.IsError)
        {
            return targetBalanceResult.FirstError;
        }

        Transaction targetTxn = targetAccount.RecordTransaction(
            forexRef, "FOREX", TransactionDirection.In, tgtAmount,
            targetBalanceResult.Value, TransactionStatus.Completed, now);

        ForexTransaction forex = forexResult.Value;
        forex.MarkExecuted(sourceTxn.Id, targetTxn.Id);

        await accountRepository.UpdateAsync(sourceAccount, cancellationToken);
        await accountRepository.UpdateAsync(targetAccount, cancellationToken);
        await forexRepository.AddAsync(forex, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        lockedRateCache.Remove(userId);

        return new ForexTransactionResponse
        {
            Id = forex.Id,
            SourceCurrency = srcCurrency.Code,
            TargetCurrency = tgtCurrency.Code,
            TargetAmount = tgtAmount.Amount,
            ExchangeRate = rate,
            Commission = commission.Amount,
            Status = ExchangeTransactionStatus.Completed
        };
    }

    public async Task<ErrorOr<List<ForexTransactionResponse>>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<ForexTransaction> transactions = await forexRepository.ListByUserIdAsync(userId, cancellationToken);

        return transactions
            .OrderByDescending(forexTransaction => forexTransaction.CreatedAt)
            .Select(forexTransaction => new ForexTransactionResponse
            {
                Id = forexTransaction.Id,
                SourceCurrency = forexTransaction.SourceAmount.Currency.Code,
                TargetCurrency = forexTransaction.TargetAmount.Currency.Code,
                TargetAmount = forexTransaction.TargetAmount.Amount,
                ExchangeRate = forexTransaction.ExchangeRate,
                Commission = forexTransaction.Commission.Amount,
                Status = forexTransaction.Status
            })
            .ToList();
    }
}
