namespace BankingApp.Contracts.Features.Transactions.Dtos;

/// <summary>Extends the history request with an export format specifier.</summary>
public sealed class TransactionExportRequest : TransactionHistoryRequest
{
    public string Format { get; set; } = string.Empty;
}