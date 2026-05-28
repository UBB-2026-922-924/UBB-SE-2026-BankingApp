namespace BankingApp.Application.Tests.Features.BillPayments;

using BankingApp.Application.Features.BillPayments;
using NodaMoney;

public sealed class BillPaymentFeePolicyTests
{
    [Fact]
    public void Calculate_WhenAmountIsAtOrBelowThreshold_ShouldReturnLowTierFee()
    {
        var currency = Currency.FromCode("USD");

        Money fee = BillPaymentFeePolicy.Calculate(50m, currency);

        fee.Should().Be(new Money(0.50m, currency));
    }

    [Fact]
    public void Calculate_WhenAmountIsAboveThreshold_ShouldReturnHighTierFee()
    {
        var currency = Currency.FromCode("USD");

        Money fee = BillPaymentFeePolicy.Calculate(100.01m, currency);

        fee.Should().Be(new Money(1.00m, currency));
    }

    [Fact]
    public void Calculate_WhenAmountEqualsThreshold_ShouldReturnLowTierFee()
    {
        var currency = Currency.FromCode("USD");

        Money fee = BillPaymentFeePolicy.Calculate(100m, currency);

        fee.Should().Be(new Money(0.50m, currency));
    }
}
