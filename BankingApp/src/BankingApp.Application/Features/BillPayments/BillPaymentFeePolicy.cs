namespace BankingApp.Application.Features.BillPayments;

using NodaMoney;

internal static class BillPaymentFeePolicy
{
    private const decimal LowTierFee = 0.50m;
    private const decimal HighTierFee = 1.00m;
    private const decimal FeeThreshold = 100m;

    internal static Money Calculate(decimal amount, Currency currency) =>
        new(amount <= FeeThreshold ? LowTierFee : HighTierFee, currency);
}
