namespace BankingApp.Application.Features.Forex.Services;

using Contracts.Features.Forex.Dtos;
using ErrorOr;

public interface IForexService
{
    public Task<ErrorOr<ForexRatePreviewResponse>> GetRatePreviewAsync(int userId, string sourceCurrency, string targetCurrency, decimal sourceAmount, CancellationToken cancellationToken = default);
    public Task<ErrorOr<ForexTransactionResponse>> ExecuteAsync(int userId, int sourceAccountId, int targetAccountId, string sourceCurrency, string targetCurrency, decimal sourceAmount, CancellationToken cancellationToken = default);
    public Task<ErrorOr<List<ForexTransactionResponse>>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default);
}
