namespace BankingApp.Application.Features.BillPayments.Services;

using Contracts.Features.BillPayments.Dtos;
using ErrorOr;

public interface IBillPaymentService
{
    public Task<ErrorOr<BillPayResponse>> ProcessAsync(int userId, int sourceAccountId, int billerId, string billerReference, decimal amount, CancellationToken cancellationToken = default);
    public Task<ErrorOr<List<AccountDto>>> GetAccountsAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<List<BillPayResponse>>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default);
}
