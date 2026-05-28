namespace BankingApp.Web.Tests.Models.Cards;

using BankingApp.Contracts.Features.Cards.Dtos;

public sealed class CardDetailsDtoTests
{
    [Fact]
    public void BalanceDisplay_WhenCurrencyIsSet_ReturnsFormattedAmountWithCurrency()
    {
        CardDetailsDto dto = new() { AccountBalance = 1250.75m, AccountCurrency = "USD" };

        dto.BalanceDisplay.Should().Be("1,250.75 USD");
    }

    [Fact]
    public void BalanceDisplay_WhenCurrencyIsEmpty_ReturnsFormattedAmountOnly()
    {
        CardDetailsDto dto = new() { AccountBalance = 500.00m, AccountCurrency = string.Empty };

        dto.BalanceDisplay.Should().Be("500.00");
    }

    [Fact]
    public void BalanceDisplay_WhenBalanceIsZero_ReturnsZeroWithCurrency()
    {
        CardDetailsDto dto = new() { AccountBalance = 0m, AccountCurrency = "EUR" };

        dto.BalanceDisplay.Should().Be("0.00 EUR");
    }
}
