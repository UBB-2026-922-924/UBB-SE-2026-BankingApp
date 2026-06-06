namespace BankingApp.Application.Features.Transactions.Services;

using Contracts.Features.Transactions.Dtos;
using ErrorOr;

/// <summary>Defines transaction history and export operations.</summary>
public interface ITransactionService
{
    public Task<ErrorOr<TransactionFilterMetadataResponse>> GetFilterMetadataAsync(int userId, CancellationToken cancellationToken = default);

    public Task<ErrorOr<TransactionHistoryResponse>> GetHistoryAsync(int userId, TransactionHistoryRequest request, CancellationToken cancellationToken = default);

    public Task<ErrorOr<TransactionDetailsResponse>> GetTransactionByIdAsync(int userId, int transactionId, CancellationToken cancellationToken = default);

    public Task<ErrorOr<TransactionExportResult>> ExportTransactionsAsync(int userId, TransactionExportRequest request, CancellationToken cancellationToken = default);

    public Task<ErrorOr<TransactionExportResult>> ExportReceiptAsync(int userId, int transactionId, CancellationToken cancellationToken = default);
}