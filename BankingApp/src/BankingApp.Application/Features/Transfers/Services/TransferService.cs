namespace BankingApp.Application.Features.Transfers.Services;

using Contracts.Features.Transfers;
using Contracts.Features.Transfers.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Aggregates.TransferAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;
using Currency = NodaMoney.Currency;
using Money = NodaMoney.Money;

public sealed class TransferService(
    IAccountRepository accountRepository,
    ITransferRepository transferRepository,
    IBeneficiaryRepository beneficiaryRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    IExchangeRateService exchangeRateService,
    ILogger<TransferService> logger)
    : ITransferService
{
    private const string TransferType = "TRANSFER";
    private const string TransferRef = "TRF";

    public async Task<ErrorOr<TransferResponse>> ExecuteAsync(
        int userId, int sourceAccountId, string recipientName, string recipientIban,
        decimal amount, string currency, string? reference, CancellationToken cancellationToken = default)
    {
        ErrorOr<Iban> ibanResult = Iban.Create(recipientIban);
        if (ibanResult.IsError)
        {
            return ibanResult.FirstError;
        }

        Currency parsedCurrency;
        try
        {
            parsedCurrency = Currency.FromCode(currency);
        }
        catch
        {
            return TransferErrors.InvalidCurrency;
        }

        Account? account = await accountRepository.GetByIdAsync(sourceAccountId, cancellationToken);
        if (account is null || account.UserId != userId)
        {
            ApplicationLogMessages.TransferAccountNotFound(logger, sourceAccountId, userId);
            return TransferErrors.AccountNotFound;
        }

        if (!account.IsActive())
        {
            ApplicationLogMessages.TransferAccountNotActive(logger, sourceAccountId);
            return TransferErrors.AccountNotActive;
        }

        if (!account.UsesCurrency(parsedCurrency))
        {
            return TransferErrors.CurrencyMismatch;
        }

        DateTime now = clock.UtcNow;
        Money transferAmount = new(amount, parsedCurrency);
        Money fee = new(TransferPricing.Fee, parsedCurrency);

        ErrorOr<Transfer> transferResult = Transfer.Create(
            userId, sourceAccountId, recipientName, ibanResult.Value, transferAmount, fee, reference, now);

        if (transferResult.IsError)
        {
            return transferResult.FirstError;
        }

        Transfer transfer = transferResult.Value;
        string transactionRef = $"{TransferRef}-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

        Account? recipientAccount = await accountRepository.GetByIbanAsync(transfer.RecipientIban.Value, cancellationToken);
        if (recipientAccount is not null && !recipientAccount.UsesCurrency(parsedCurrency))
        {
            return TransferErrors.CurrencyMismatch;
        }

        ErrorOr<Money> newBalanceResult = account.Debit(transfer.TotalDebit, now);
        if (newBalanceResult.IsError)
        {
            ApplicationLogMessages.TransferInsufficientFunds(logger, sourceAccountId);
            return newBalanceResult.FirstError;
        }

        Transaction transaction = account.RecordTransaction(
            transactionRef, TransferType, TransactionDirection.Out,
            transfer.Amount, newBalanceResult.Value, TransactionStatus.Completed, now);

        transfer.MarkExecuted(transaction.Id, now.AddDays(1));

        if (recipientAccount is not null && recipientAccount.Id != account.Id)
        {
            ErrorOr<Money> recipientBalanceResult = recipientAccount.Credit(transfer.Amount, now);
            if (recipientBalanceResult.IsError)
            {
                return recipientBalanceResult.FirstError;
            }

            recipientAccount.RecordTransaction(
                $"{transactionRef}-IN",
                TransferType,
                TransactionDirection.In,
                transfer.Amount,
                recipientBalanceResult.Value,
                TransactionStatus.Completed,
                now);
        }

        await UpdateBeneficiaryStatsAsync(userId, transfer.RecipientIban, transfer.Amount.Amount, now, cancellationToken);
        await accountRepository.UpdateAsync(account, cancellationToken);
        if (recipientAccount is not null && recipientAccount.Id != account.Id)
        {
            await accountRepository.UpdateAsync(recipientAccount, cancellationToken);
        }

        await transferRepository.AddAsync(transfer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TransferResponse
        {
            Id = transfer.Id,
            SourceAccountId = transfer.SourceAccountId,
            TransactionId = transfer.LedgerTransactionId,
            TransactionRef = transactionRef,
            RecipientName = transfer.RecipientName,
            RecipientIban = transfer.RecipientIban.Value,
            Amount = transfer.Amount.Amount,
            Currency = transfer.Amount.Currency.Code,
            Fee = transfer.Fee.Amount,
            Reference = transfer.Reference,
            Status = transfer.Status,
            CreatedAt = transfer.CreatedAt
        };
    }

    public async Task<ErrorOr<List<TransferAccountSelectionResponse>>> GetAccountsAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(userId, cancellationToken);

        return accounts
            .Where(account => account.IsActive())
            .Select(account => new TransferAccountSelectionResponse
            {
                Id = account.Id,
                Iban = account.Iban.Value,
                Currency = account.Balance.Currency.Code,
                Balance = account.Balance.Amount,
                AccountName = account.AccountName ?? string.Empty
            })
            .ToList();
    }

    public async Task<ErrorOr<List<TransferResponse>>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Transfer> transfers = await transferRepository.ListByUserIdAsync(userId, cancellationToken);

        return transfers
            .OrderByDescending(transfer => transfer.CreatedAt)
            .Select(transfer => new TransferResponse
            {
                Id = transfer.Id,
                SourceAccountId = transfer.SourceAccountId,
                TransactionId = transfer.LedgerTransactionId,
                RecipientName = transfer.RecipientName,
                RecipientIban = transfer.RecipientIban.Value,
                RecipientBankName = transfer.RecipientBankName,
                Amount = transfer.Amount.Amount,
                Currency = transfer.Amount.Currency.Code,
                Fee = transfer.Fee.Amount,
                Reference = transfer.Reference,
                Status = transfer.Status,
                CreatedAt = transfer.CreatedAt
            })
            .ToList();
    }

    public Task<ErrorOr<TransferForexPreviewResponse>> GetFxPreviewAsync(
        string sourceCurrency, string targetCurrency, decimal amount, CancellationToken cancellationToken = default)
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
            return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(ForexErrors.InvalidCurrency);
        }

        ErrorOr<decimal> rateResult = exchangeRateService.GetRate(source, target);
        if (rateResult.IsError)
        {
            return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(rateResult.FirstError);
        }

        decimal rate = rateResult.Value;
        return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(new TransferForexPreviewResponse
        {
            ExchangeRate = rate,
            ConvertedAmount = Math.Round(amount * rate, 2)
        });
    }

    public Task<ErrorOr<TransferIbanValidationResponse>> ValidateIbanAsync(string iban, CancellationToken cancellationToken = default)
    {
        ErrorOr<Iban> result = Iban.Create(iban);
        return Task.FromResult<ErrorOr<TransferIbanValidationResponse>>(new TransferIbanValidationResponse
        {
            IsValid = !result.IsError,
            BankName = result.IsError ? string.Empty : InferBankName(iban)
        });
    }

    private static string InferBankName(string iban)
    {
        if (iban.Length < 2)
        {
            return string.Empty;
        }

        return iban[..2].ToUpperInvariant() switch
        {
            "RO" => "Romanian Bank",
            "DE" => "German Bank",
            "GB" => "UK Bank",
            "FR" => "French Bank",
            _ => string.Empty
        };
    }

    private async Task UpdateBeneficiaryStatsAsync(int userId, Iban iban, decimal amount, DateTime now, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Beneficiary> beneficiaries = await beneficiaryRepository.ListByUserIdAsync(userId, cancellationToken);
        Beneficiary? beneficiary = beneficiaries.FirstOrDefault(b => b.Iban.Value == iban.Value);
        if (beneficiary is not null)
        {
            beneficiary.RegisterTransfer(amount, now);
            await beneficiaryRepository.UpdateAsync(beneficiary, cancellationToken);
        }
    }
}
