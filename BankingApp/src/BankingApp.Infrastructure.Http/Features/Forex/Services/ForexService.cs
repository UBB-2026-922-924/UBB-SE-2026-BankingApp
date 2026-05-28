namespace BankingApp.Infrastructure.Http.Features.Forex.Services;

using Application.Shared.Http;
using Contracts.Features.Forex.Dtos;
using Contracts.Features.Forex.Services;
using Contracts.Http;
using ErrorOr;

public sealed class ForexService(IApiClient apiClient) : IForexService
{
    public Task<ErrorOr<ForexRatePreviewResponse>> GetPreviewAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken ct = default)
        => apiClient.GetAsync<ForexRatePreviewResponse>(
            $"{ApiEndpoints.Forex.PreviewFull}?sourceCurrency={Uri.EscapeDataString(fromCurrency)}&targetCurrency={Uri.EscapeDataString(toCurrency)}&amount={amount}", ct);

    public Task<ErrorOr<ForexTransactionResponse>> ExecuteAsync(ForexTransactionRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<ForexTransactionRequest, ForexTransactionResponse>(ApiEndpoints.Forex.ExecuteFull, request, ct);

    public Task<ErrorOr<List<ForexTransactionResponse>>> GetHistoryAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<ForexTransactionResponse>>(ApiEndpoints.Forex.HistoryFull, ct);
}
