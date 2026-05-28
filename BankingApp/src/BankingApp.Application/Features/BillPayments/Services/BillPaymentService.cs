namespace BankingApp.Application.Features.BillPayments.Services;

using Contracts.Features.BillPayments.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Aggregates.BillPaymentAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.ReferenceData.Billers;
using Domain.Repositories;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;
using Money = NodaMoney.Money;

public sealed class BillPaymentService(
    IAccountRepository accountRepository,
    IBillPaymentRepository billPaymentRepository,
    IBillerRepository billerRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock)
    : IBillPaymentService
{
    public async Task<ErrorOr<BillPayResponse>> ProcessAsync(
        int userId, int sourceAccountId, int billerId, string billerReference, decimal amount, CancellationToken cancellationToken = default)
    {
        Account? account = await accountRepository.GetByIdAsync(sourceAccountId, cancellationToken);
        if (account is null || account.UserId != userId)
        {
            return AccountErrors.NotFound;
        }

        if (!account.IsActive())
        {
            return BillPaymentErrors.AccountNotActive;
        }

        Biller? biller = await billerRepository.GetByIdAsync(billerId, cancellationToken);
        if (biller is null)
        {
            return BillPaymentErrors.NotFound;
        }

        DateTime now = clock.UtcNow;

        ErrorOr<BillPayment> paymentResult = BillPayment.Create(
            userId,
            sourceAccountId,
            billerId,
            billerReference,
            new Money(amount, account.Currency),
            BillPaymentFeePolicy.Calculate(amount, account.Currency),
            now);

        if (paymentResult.IsError)
        {
            return paymentResult.FirstError;
        }

        BillPayment payment = paymentResult.Value;
        string receiptNumber = $"RCP-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

        ErrorOr<Money> newBalanceResult = account.Debit(payment.TotalDebit, now);
        if (newBalanceResult.IsError)
        {
            return newBalanceResult.FirstError;
        }

        Transaction txn = account.RecordTransaction(
            receiptNumber,
            "BILL_PAYMENT",
            TransactionDirection.Out,
            payment.Amount,
            newBalanceResult.Value,
            TransactionStatus.Completed,
            now);

        payment.MarkProcessed(receiptNumber, txn.Id);

        await accountRepository.UpdateAsync(account, cancellationToken);
        await billPaymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BillPayResponse
        {
            Id = payment.Id,
            ReceiptNumber = payment.ReceiptNumber,
            Fee = payment.Fee.Amount,
            Amount = payment.Amount.Amount,
            Status = payment.Status.ToString(),
            CreatedAt = payment.CreatedAt
        };
    }

    public async Task<ErrorOr<List<AccountDto>>> GetAccountsAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(userId, cancellationToken);

        return accounts
            .Where(account => account.IsActive())
            .Select(account =>
            {
                Card? primaryCard = account.Cards.FirstOrDefault(card => card.IsActive());
                string? lastFour = primaryCard is not null && primaryCard.CardNumber.Length >= 4
                    ? primaryCard.CardNumber[^4..]
                    : null;

                return new AccountDto
                {
                    Id = account.Id,
                    Iban = account.Iban.Value,
                    Currency = account.Balance.Currency.Code,
                    Balance = account.Balance.Amount,
                    AccountName = account.AccountName ?? string.Empty,
                    CardLastFourDigits = lastFour
                };
            })
            .ToList();
    }

    public async Task<ErrorOr<List<BillPayResponse>>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<BillPayment> payments = await billPaymentRepository.ListByUserIdAsync(userId, cancellationToken);

        return payments
            .OrderByDescending(billPayment => billPayment.CreatedAt)
            .Select(billPayment => new BillPayResponse
            {
                Id = billPayment.Id,
                ReceiptNumber = billPayment.ReceiptNumber,
                Fee = billPayment.Fee.Amount,
                Amount = billPayment.Amount.Amount,
                Status = billPayment.Status.ToString(),
                CreatedAt = billPayment.CreatedAt
            })
            .ToList();
    }
}
