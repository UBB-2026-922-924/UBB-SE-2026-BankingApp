namespace BankingApp.Application.Features.Transfers.Services;

using Contracts.Features.Transfers.Dtos;
using ErrorOr;

public interface ITransferService
{
    public Task<ErrorOr<TransferResponse>> ExecuteAsync(int userId, int sourceAccountId, string recipientName, string recipientIban, decimal amount, string currency, string? reference, CancellationToken cancellationToken = default);
    public Task<ErrorOr<List<TransferAccountSelectionResponse>>> GetAccountsAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<List<TransferResponse>>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<TransferForexPreviewResponse>> GetFxPreviewAsync(string sourceCurrency, string targetCurrency, decimal amount, CancellationToken cancellationToken = default);
    public Task<ErrorOr<TransferIbanValidationResponse>> ValidateIbanAsync(string iban, CancellationToken cancellationToken = default);
}
