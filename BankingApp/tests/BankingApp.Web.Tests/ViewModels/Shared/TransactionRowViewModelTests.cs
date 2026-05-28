namespace BankingApp.Web.Tests.ViewModels.Shared;

using BankingApp.Web.Models.BillPayments;
using BankingApp.Web.ViewModels.Shared;

public sealed class TransactionRowViewModelTests
{
    [Theory]
    [InlineData("completed",  "bg-success")]
    [InlineData("Completed",  "bg-success")]
    [InlineData("pending",    "bg-warning text-dark")]
    [InlineData("failed",     "bg-danger")]
    [InlineData("cancelled",  "bg-secondary")]
    [InlineData("unknown",    "bg-secondary")]
    public void StatusBadgeCssClass_ReturnsExpectedClass(string status, string expectedClass)
    {
        TransactionRowViewModel viewModel = new() { Status = status };

        viewModel.StatusBadgeCssClass.Should().Be(expectedClass);
    }

    [Fact]
    public void AmountCssClass_WhenDebit_ReturnsDebitClass()
    {
        TransactionRowViewModel viewModel = new() { IsDebit = true };

        viewModel.AmountCssClass.Should().Be("amount-debit");
    }

    [Fact]
    public void AmountCssClass_WhenCredit_ReturnsCreditClass()
    {
        TransactionRowViewModel viewModel = new() { IsDebit = false };

        viewModel.AmountCssClass.Should().Be("amount-credit");
    }

    [Fact]
    public void FromBillPayment_MapsAllFields()
    {
        var billPayment = new BillPaymentRowModel()
        {
            ReceiptNumber = "RCP-001",
            Amount        = 100m,
            Fee           = 1.50m,
            Status        = "Completed",
            CreatedAt     = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        var result = TransactionRowViewModel.FromBillPayment(billPayment);

        result.Description.Should().Be("RCP-001");
        result.Amount.Should().Be(101.50m);
        result.Status.Should().Be("Completed");
        result.OccurredAt.Should().Be(new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc));
        result.IsDebit.Should().BeTrue();
        result.CurrencyCode.Should().BeNull();
    }
}
