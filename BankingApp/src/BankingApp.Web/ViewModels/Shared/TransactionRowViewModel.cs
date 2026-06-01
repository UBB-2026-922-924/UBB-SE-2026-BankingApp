namespace BankingApp.Web.ViewModels.Shared;

using Models.BillPayments;

public sealed class TransactionRowViewModel
{
    public DateTime OccurredAt { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? CurrencyCode { get; init; }
    public bool IsDebit { get; init; }
    public string Status { get; init; } = string.Empty;

    public string AmountCssClass => IsDebit ? "amount-debit" : "amount-credit";

    public string StatusBadgeCssClass => Status.ToLowerInvariant() switch
    {
        "completed" => "bg-success",
        "pending"   => "bg-warning text-dark",
        "failed"    => "bg-danger",
        "cancelled" => "bg-secondary",
        _           => "bg-secondary"
    };

    public static TransactionRowViewModel FromBillPayment(BillPaymentRowModel billPayment) =>
        new()
        {
            OccurredAt  = billPayment.CreatedAt,
            Description = billPayment.ReceiptNumber,
            Amount      = billPayment.Total,
            IsDebit     = true,
            Status      = billPayment.Status
        };
}
