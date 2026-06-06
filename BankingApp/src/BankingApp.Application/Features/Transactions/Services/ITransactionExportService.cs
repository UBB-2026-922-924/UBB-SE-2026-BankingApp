namespace BankingApp.Application.Features.Transactions.Services;

using Contracts.Features.Transactions.Dtos;

/// <summary>
///     Generates exportable file content from transaction data.
///     Implemented in infrastructure; consumed by the application layer.
/// </summary>
public interface ITransactionExportService
{
    public TransactionExportResult ExportStatement(
        IReadOnlyCollection<TransactionHistoryItemDto> transactions,
        TransactionHistoryRequest request,
        string format);

    public TransactionExportResult ExportReceipt(TransactionHistoryItemDto transaction);
}