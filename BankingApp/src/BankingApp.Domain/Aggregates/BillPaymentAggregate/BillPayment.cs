namespace BankingApp.Domain.Aggregates.BillPaymentAggregate;

using Common.Errors;
using Common.Primitives;
using Enums;
using ErrorOr;
using Money = NodaMoney.Money;

public sealed class BillPayment : AggregateRoot<int>
{
    private BillPayment()
    {
    }

    public int UserId { get; private set; }

    public int SourceAccountId { get; private set; }

    public int BillerId { get; private set; }

    public int? LedgerTransactionId { get; private set; }

    public string BillerReference { get; private set; } = string.Empty;

    public Money Amount { get; private set; } = default!;

    public Money Fee { get; private set; } = default!;

    public string ReceiptNumber { get; private set; } = string.Empty;

    public BillPaymentStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Money TotalDebit => Amount + Fee;

    public static ErrorOr<BillPayment> Create(
        int userId,
        int sourceAccountId,
        int billerId,
        string billerReference,
        Money amount,
        Money fee,
        DateTime createdAt)
    {
        if (amount.Amount <= 0)
        {
            return BillPaymentErrors.InvalidAmount;
        }

        if (fee.Amount < 0)
        {
            return BillPaymentErrors.InvalidFee;
        }

        if (amount.Currency != fee.Currency)
        {
            return AccountErrors.CurrencyMismatch;
        }

        if (string.IsNullOrWhiteSpace(billerReference))
        {
            return BillPaymentErrors.InvalidReference;
        }

        return new BillPayment
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            BillerId = billerId,
            BillerReference = billerReference.Trim(),
            Amount = amount,
            Fee = fee,
            Status = BillPaymentStatus.Pending,
            CreatedAt = createdAt
        };
    }

    public void MarkProcessed(string receiptNumber, int? ledgerTransactionId)
    {
        ReceiptNumber = receiptNumber;
        LedgerTransactionId = ledgerTransactionId;
        Status = BillPaymentStatus.Completed;
    }
}
