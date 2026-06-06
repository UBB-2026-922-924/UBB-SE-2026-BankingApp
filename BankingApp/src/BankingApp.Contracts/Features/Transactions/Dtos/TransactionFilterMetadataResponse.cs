namespace BankingApp.Contracts.Features.Transactions.Dtos;

/// <summary>Response returned by the transaction filter metadata endpoint.</summary>
public sealed class TransactionFilterMetadataResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AccountFilterOptionDto> Accounts { get; set; } = [];
    public List<CardFilterOptionDto> Cards { get; set; } = [];
    public List<string> AvailableTransactionTypes { get; set; } = [];
    public List<string> AvailableStatuses { get; set; } = [];
    public List<string> AvailableDirections { get; set; } = [];
}