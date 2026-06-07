namespace BankingApp.Contracts.Features.Transactions.Services;

using Dtos;
using ErrorOr;

public interface ITransactionService
{
    public Task<TransactionFilterMetadataResponse?> GetFilterMetadataAsync(CancellationToken ct = default);
    public Task<TransactionHistoryResponse?> GetHistoryAsync(TransactionHistoryRequest request, CancellationToken ct = default);
    public Task<TransactionDetailsResponse?> GetTransactionByIdAsync(int transactionId, CancellationToken ct = default);
    public Task<ExportedFileResult?> ExportTransactionsAsync(TransactionExportRequest request, CancellationToken ct = default);
    public Task<ExportedFileResult?> ExportReceiptAsync(int transactionId, CancellationToken ct = default);
}
