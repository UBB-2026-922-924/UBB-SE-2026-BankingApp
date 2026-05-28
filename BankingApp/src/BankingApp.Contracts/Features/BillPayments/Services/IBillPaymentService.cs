namespace BankingApp.Contracts.Features.BillPayments.Services;

using Dtos;
using ErrorOr;

public interface IBillPaymentService
{
    public Task<ErrorOr<List<AccountDto>>> GetAccountsAsync(CancellationToken ct = default);
    public Task<ErrorOr<FeeResponse>> GetFeeAsync(decimal amount, CancellationToken ct = default);
    public Task<ErrorOr<BillPayResponse>> PayBillAsync(BillPayRequest request, CancellationToken ct = default);
    public Task<ErrorOr<List<BillPayResponse>>> GetHistoryAsync(CancellationToken ct = default);
}
