namespace BankingApp.Contracts.Features.Forex.Services;

using Dtos;
using ErrorOr;

public interface IForexService
{
    public Task<ErrorOr<ForexRatePreviewResponse>> GetPreviewAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken ct = default);
    public Task<ErrorOr<ForexTransactionResponse>> ExecuteAsync(ForexTransactionRequest request, CancellationToken ct = default);
    public Task<ErrorOr<List<ForexTransactionResponse>>> GetHistoryAsync(CancellationToken ct = default);
}
