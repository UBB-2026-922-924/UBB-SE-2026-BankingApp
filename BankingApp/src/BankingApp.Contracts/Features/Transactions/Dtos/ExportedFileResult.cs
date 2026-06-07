namespace BankingApp.Contracts.Features.Transactions.Dtos;

public sealed class ExportedFileResult
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}