namespace BankingApp.Contracts.Features.Transactions.Dtos;

/// <summary>Encapsulates filter, sort, and export format parameters for a transaction export request.</summary>
public sealed class TransactionExportRequest
{
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? TransactionType { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public int? AccountId { get; set; }
    public int? CardId { get; set; }
    public string? Status { get; set; }
    public string? Direction { get; set; }
    public string SortField { get; set; } = TransactionSortFields.Date;
    public string SortDirection { get; set; } = SortDirections.Desc;
    public string Format { get; set; } = string.Empty;
}