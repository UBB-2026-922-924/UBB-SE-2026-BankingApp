namespace BankingApp.Domain.Tests.Aggregates.BillPaymentAggregate;

using BankingApp.Domain.Aggregates.BillPaymentAggregate;
using Common.Errors;
using Enums;
using ErrorOr;
using NodaMoney;

public sealed class BillPaymentTests
{
    [Fact]
    public void Create_WhenAmountIsZero_ShouldReturnInvalidAmountError()
    {
        var amount = new Money(0m, Currency.FromCode("USD"));
        var fee = new Money(1m, Currency.FromCode("USD"));

        ErrorOr<BillPayment> result = BillPayment.Create(1, 1, 1, "REF123", amount, fee, DateTime.UtcNow);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.InvalidAmount);
    }

    [Fact]
    public void Create_WhenAmountIsNegative_ShouldReturnInvalidAmountError()
    {
        var amount = new Money(-5m, Currency.FromCode("USD"));
        var fee = new Money(1m, Currency.FromCode("USD"));

        ErrorOr<BillPayment> result = BillPayment.Create(1, 1, 1, "REF123", amount, fee, DateTime.UtcNow);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.InvalidAmount);
    }

    [Fact]
    public void Create_WhenFeeIsNegative_ShouldReturnInvalidFeeError()
    {
        var amount = new Money(10m, Currency.FromCode("USD"));
        var fee = new Money(-1m, Currency.FromCode("USD"));

        ErrorOr<BillPayment> result = BillPayment.Create(1, 1, 1, "REF123", amount, fee, DateTime.UtcNow);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.InvalidFee);
    }

    [Fact]
    public void Create_WhenCurrenciesMismatch_ShouldReturnCurrencyMismatchError()
    {
        var amount = new Money(10m, Currency.FromCode("USD"));
        var fee = new Money(1m, Currency.FromCode("EUR"));

        ErrorOr<BillPayment> result = BillPayment.Create(1, 1, 1, "REF123", amount, fee, DateTime.UtcNow);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.CurrencyMismatch);
    }

    [Fact]
    public void Create_WhenBillerReferenceIsEmpty_ShouldReturnInvalidReferenceError()
    {
        var amount = new Money(10m, Currency.FromCode("USD"));
        var fee = new Money(1m, Currency.FromCode("USD"));

        ErrorOr<BillPayment> result = BillPayment.Create(1, 1, 1, string.Empty, amount, fee, DateTime.UtcNow);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.InvalidReference);
    }

    [Fact]
    public void Create_WhenBillerReferenceIsWhitespace_ShouldReturnInvalidReferenceError()
    {
        var amount = new Money(10m, Currency.FromCode("USD"));
        var fee = new Money(1m, Currency.FromCode("USD"));

        ErrorOr<BillPayment> result = BillPayment.Create(1, 1, 1, "   ", amount, fee, DateTime.UtcNow);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.InvalidReference);
    }

    [Fact]
    public void Create_WhenValidParams_ShouldCreateWithPendingStatus()
    {
        var amount = new Money(10m, Currency.FromCode("USD"));
        var fee = new Money(1m, Currency.FromCode("USD"));
        DateTime createdAt = DateTime.UtcNow;

        ErrorOr<BillPayment> result = BillPayment.Create(1, 2, 3, " REF123 ", amount, fee, createdAt);

        result.IsError.Should().BeFalse();
        result.Value.UserId.Should().Be(1);
        result.Value.SourceAccountId.Should().Be(2);
        result.Value.BillerId.Should().Be(3);
        result.Value.BillerReference.Should().Be("REF123");
        result.Value.Amount.Should().Be(amount);
        result.Value.Fee.Should().Be(fee);
        result.Value.Status.Should().Be(BillPaymentStatus.Pending);
        result.Value.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void MarkProcessed_WhenCalled_ShouldSetStatusToCompleted()
    {
        BillPayment payment = BillPayment.Create(1, 1, 1, "REF", new Money(10m, "USD"), new Money(1m, "USD"), DateTime.UtcNow).Value;

        payment.MarkProcessed("RCP-123", 456);

        payment.Status.Should().Be(BillPaymentStatus.Completed);
    }

    [Fact]
    public void MarkProcessed_WhenCalled_ShouldSetReceiptNumber()
    {
        BillPayment payment = BillPayment.Create(1, 1, 1, "REF", new Money(10m, "USD"), new Money(1m, "USD"), DateTime.UtcNow).Value;

        payment.MarkProcessed("RCP-123", 456);

        payment.ReceiptNumber.Should().Be("RCP-123");
    }

    [Fact]
    public void MarkProcessed_WhenCalled_ShouldSetLedgerTransactionId()
    {
        BillPayment payment = BillPayment.Create(1, 1, 1, "REF", new Money(10m, "USD"), new Money(1m, "USD"), DateTime.UtcNow).Value;

        payment.MarkProcessed("RCP-123", 456);

        payment.LedgerTransactionId.Should().Be(456);
    }
}
