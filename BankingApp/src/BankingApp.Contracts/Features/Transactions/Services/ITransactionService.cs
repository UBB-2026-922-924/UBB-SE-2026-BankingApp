namespace BankingApp.Contracts.Features.Transactions.Services;

using Dtos;
using ErrorOr;

public interface ITransactionService
{
    public Task<ErrorOr<TransactionFilterMetadataResponse>> GetFilterMetadataAsync(CancellationToken ct = default);
    public Task<ErrorOr<TransactionHistoryResponse>> GetHistoryAsync(TransactionHistoryRequest request, CancellationToken ct = default);
    public Task<ErrorOr<TransactionDetailsResponse>> GetTransactionByIdAsync(int transactionId, CancellationToken ct = default);
    public Task<ErrorOr<TransactionExportResult>> ExportTransactionsAsync(TransactionExportRequest request, CancellationToken ct = default);
    public Task<ErrorOr<TransactionExportResult>> ExportReceiptAsync(int transactionId, CancellationToken ct = default);
}
