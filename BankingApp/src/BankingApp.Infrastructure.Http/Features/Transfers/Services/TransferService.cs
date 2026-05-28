namespace BankingApp.Infrastructure.Http.Features.Transfers.Services;

using Application.Shared.Http;
using Contracts.Features.Transfers.Dtos;
using Contracts.Features.Transfers.Services;
using Contracts.Http;
using ErrorOr;

public sealed class TransferService(IApiClient apiClient) : ITransferService
{
    public Task<ErrorOr<List<TransferResponse>>> GetHistoryAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<TransferResponse>>(ApiEndpoints.Transfers.Base, ct);

    public Task<ErrorOr<List<TransferAccountSelectionResponse>>> GetAccountsAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<TransferAccountSelectionResponse>>(ApiEndpoints.Transfers.AccountsFull, ct);

    public Task<ErrorOr<TransferIbanValidationResponse>> ValidateIbanAsync(TransferIbanValidationRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<TransferIbanValidationRequest, TransferIbanValidationResponse>(ApiEndpoints.Transfers.ValidateIbanFull, request, ct);

    public Task<ErrorOr<TransferForexPreviewResponse>> GetFxPreviewAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken ct = default)
        => apiClient.GetAsync<TransferForexPreviewResponse>(
            $"{ApiEndpoints.Transfers.FxPreviewFull}?from={Uri.EscapeDataString(fromCurrency)}&to={Uri.EscapeDataString(toCurrency)}&amount={amount}", ct);

    public Task<ErrorOr<TransferExecutionResponse>> ExecuteAsync(CreateTransferRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<CreateTransferRequest, TransferExecutionResponse>(ApiEndpoints.Transfers.ExecuteFull, request, ct);
}
