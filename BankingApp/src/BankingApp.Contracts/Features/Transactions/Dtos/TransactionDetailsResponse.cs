namespace BankingApp.Contracts.Features.Transactions.Dtos;

/// <summary>Response returned by the single transaction detail endpoint.</summary>
public sealed class TransactionDetailsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TransactionHistoryItemDto? Transaction { get; set; }
}