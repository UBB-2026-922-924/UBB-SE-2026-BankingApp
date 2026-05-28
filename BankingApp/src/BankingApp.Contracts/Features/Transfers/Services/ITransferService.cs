namespace BankingApp.Contracts.Features.Transfers.Services;

using Dtos;
using ErrorOr;

public interface ITransferService
{
    public Task<ErrorOr<List<TransferResponse>>> GetHistoryAsync(CancellationToken ct = default);
    public Task<ErrorOr<List<TransferAccountSelectionResponse>>> GetAccountsAsync(CancellationToken ct = default);
    public Task<ErrorOr<TransferIbanValidationResponse>> ValidateIbanAsync(TransferIbanValidationRequest request, CancellationToken ct = default);
    public Task<ErrorOr<TransferForexPreviewResponse>> GetFxPreviewAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken ct = default);
    public Task<ErrorOr<TransferExecutionResponse>> ExecuteAsync(CreateTransferRequest request, CancellationToken ct = default);
}
