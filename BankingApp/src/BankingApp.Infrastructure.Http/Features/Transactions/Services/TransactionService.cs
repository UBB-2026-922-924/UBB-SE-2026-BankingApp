namespace BankingApp.Infrastructure.Http.Features.Transactions.Services;

using Application.Shared.Http;
using Contracts.Features.Transactions.Dtos;
using Contracts.Features.Transactions.Services;
using Contracts.Http;
using ErrorOr;

public sealed class TransactionService(IApiClient apiClient) : ITransactionService
{
    public Task<ErrorOr<TransactionExportResult>> ExportReceiptAsync(int transactionId, CancellationToken ct = default)
        => apiClient.GetAsync<TransactionExportResult>(ApiEndpoints.Transactions.ByReceiptFull(transactionId), ct);

    public Task<ErrorOr<TransactionExportResult>> ExportTransactionsAsync(TransactionExportRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<TransactionExportRequest, TransactionExportResult>($"{ApiEndpoints.Transactions.Base}/{ApiEndpoints.Transactions.Export}", request, ct);

    public Task<ErrorOr<TransactionFilterMetadataResponse>> GetFilterMetadataAsync(CancellationToken ct = default)
        => apiClient.GetAsync<TransactionFilterMetadataResponse>($"{ApiEndpoints.Transactions.Base}/{ApiEndpoints.Transactions.Filters}", ct);

    public Task<ErrorOr<TransactionHistoryResponse>> GetHistoryAsync(TransactionHistoryRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<TransactionHistoryRequest, TransactionHistoryResponse>($"{ApiEndpoints.Transactions.Base}/{ApiEndpoints.Transactions.History}", request, ct);

    public Task<ErrorOr<TransactionDetailsResponse>> GetTransactionByIdAsync(int transactionId, CancellationToken ct = default)
        => apiClient.GetAsync<TransactionDetailsResponse>(ApiEndpoints.Transactions.ByIdFull(transactionId), ct);
}
