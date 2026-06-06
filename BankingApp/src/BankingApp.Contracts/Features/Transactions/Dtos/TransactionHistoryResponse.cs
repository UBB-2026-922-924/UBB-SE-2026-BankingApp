namespace BankingApp.Contracts.Features.Transactions.Dtos;

/// <summary>Response returned by the transaction history endpoint.</summary>
public sealed class TransactionHistoryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TransactionHistoryRequest? AppliedFilters { get; set; }
    public List<TransactionHistoryItemDto> Transactions { get; set; } = [];
}