namespace BankingApp.Infrastructure.Http.Features.BillPayments.Services;

using Application.Shared.Http;
using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.BillPayments.Services;
using Contracts.Http;
using ErrorOr;

public sealed class BillPaymentService(IApiClient apiClient) : IBillPaymentService
{
    public Task<ErrorOr<List<AccountDto>>> GetAccountsAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<AccountDto>>(ApiEndpoints.BillPayments.AccountsFull, ct);

    public Task<ErrorOr<FeeResponse>> GetFeeAsync(decimal amount, CancellationToken ct = default)
        => apiClient.GetAsync<FeeResponse>($"{ApiEndpoints.BillPayments.FeeFull}?amount={amount}", ct);

    public Task<ErrorOr<BillPayResponse>> PayBillAsync(BillPayRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<BillPayRequest, BillPayResponse>(ApiEndpoints.BillPayments.PayFull, request, ct);

    public Task<ErrorOr<List<BillPayResponse>>> GetHistoryAsync(CancellationToken ct = default)
        => apiClient.GetAsync<List<BillPayResponse>>(ApiEndpoints.BillPayments.HistoryFull, ct);
}
