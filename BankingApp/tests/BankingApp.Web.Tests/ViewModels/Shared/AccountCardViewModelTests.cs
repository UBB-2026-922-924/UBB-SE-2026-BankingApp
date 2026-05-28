namespace BankingApp.Web.Tests.ViewModels.Shared;

using BankingApp.Contracts.Features.AccountOverview.Dtos;
using BankingApp.Domain.Enums;
using BankingApp.Web.ViewModels.Shared;

public sealed class AccountCardViewModelTests
{
    [Fact]
    public void IsActive_WhenStatusIsActive_ReturnsTrue()
    {
        AccountCardViewModel viewModel = new() { Status = "Active" };

        viewModel.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("Frozen")]
    [InlineData("Cancelled")]
    [InlineData("Expired")]
    public void IsActive_WhenStatusIsNotActive_ReturnsFalse(string status)
    {
        AccountCardViewModel viewModel = new() { Status = status };

        viewModel.IsActive.Should().BeFalse();
    }

    [Fact]
    public void FromCard_MapsAllFields()
    {
        var card = new CardDto()
        {
            AccountName     = "Checking Account",
            CardNumber      = "**** **** **** 1234",
            AccountBalance  = 2500.75m,
            CardType        = CardType.Debit,
            Status          = CardStatus.Active
        };

        var result = AccountCardViewModel.FromCard(card);

        result.AccountName.Should().Be("Checking Account");
        result.MaskedCardNumber.Should().Be("**** **** **** 1234");
        result.Balance.Should().Be(2500.75m);
        result.CardType.Should().Be("Debit");
        result.Status.Should().Be("Active");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void FromCard_WhenAccountNameIsNull_UsesEmptyString()
    {
        var card = new CardDto() { AccountName = null, Status = CardStatus.Frozen };

        var result = AccountCardViewModel.FromCard(card);

        result.AccountName.Should().BeEmpty();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void FromCard_WhenAccountBalanceIsNull_UsesZero()
    {
        var card = new CardDto() { AccountBalance = null, Status = CardStatus.Active };

        var result = AccountCardViewModel.FromCard(card);

        result.Balance.Should().Be(0m);
    }
}
