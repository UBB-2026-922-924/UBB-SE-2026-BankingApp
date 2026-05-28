namespace BankingApp.Domain.Aggregates.TransferAggregate;

using Common.Errors;
using Common.Primitives;
using Enums;
using ErrorOr;
using ValueObjects;
using Money = NodaMoney.Money;

public sealed class Transfer : AggregateRoot<int>
{
    private Transfer()
    {
    }

    public int UserId { get; private set; }

    public int SourceAccountId { get; private set; }

    public int? LedgerTransactionId { get; private set; }

    public string RecipientName { get; private set; } = string.Empty;

    public Iban RecipientIban { get; private set; } = default!;

    public string? RecipientBankName { get; private set; }

    public Money Amount { get; private set; } = default!;

    public Money? ConvertedAmount { get; private set; }

    public decimal? ExchangeRate { get; private set; }

    public Money Fee { get; private set; } = default!;

    public string? Reference { get; private set; }

    public TransferStatus Status { get; private set; }

    public DateTime? EstimatedArrival { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Money TotalDebit => Amount + Fee;

    public static ErrorOr<Transfer> Create(
        int userId,
        int sourceAccountId,
        string recipientName,
        Iban recipientIban,
        Money amount,
        Money fee,
        string? reference,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(recipientName))
        {
            return TransferErrors.InvalidRecipientName;
        }

        if (amount.Amount <= 0)
        {
            return TransferErrors.InvalidAmount;
        }

        if (fee.Amount < 0)
        {
            return TransferErrors.InvalidFee;
        }

        if (amount.Currency != fee.Currency)
        {
            return TransferErrors.CurrencyMismatch;
        }

        return new Transfer
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            RecipientName = recipientName.Trim(),
            RecipientIban = recipientIban,
            Amount = amount,
            Fee = fee,
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
            Status = TransferStatus.Pending,
            CreatedAt = createdAt
        };
    }

    public void MarkExecuted(int? ledgerTransactionId, DateTime? estimatedArrival)
    {
        LedgerTransactionId = ledgerTransactionId;
        EstimatedArrival = estimatedArrival;
        Status = TransferStatus.Completed;
    }

    public void MarkFailed()
    {
        Status = TransferStatus.Failed;
    }
}
