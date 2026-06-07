namespace BankingApp.Contracts.Features.Transactions.Dtos;

/// <summary>Holds the raw bytes and metadata for a generated export file.</summary>
public sealed class TransactionExportResult
{
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}