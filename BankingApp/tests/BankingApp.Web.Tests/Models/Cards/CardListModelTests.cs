namespace BankingApp.Web.Tests.Models.Cards;

using BankingApp.Contracts.Features.Cards.Dtos;
using BankingApp.Web.Models.Cards;

public sealed class CardListModelTests
{
    [Fact]
    public void HasCards_WhenCardsListIsEmpty_ShouldReturnFalse()
    {
        CardListModel model = new() { Cards = [] };

        model.HasCards.Should().BeFalse();
    }

    [Fact]
    public void HasCards_WhenCardsListHasOneEntry_ShouldReturnTrue()
    {
        CardListModel model = new()
        {
            Cards = [new CardDetailsDto()]
        };

        model.HasCards.Should().BeTrue();
    }

    [Fact]
    public void HasCards_WhenCardsListHasMultipleEntries_ShouldReturnTrue()
    {
        CardListModel model = new()
        {
            Cards = [new CardDetailsDto(), new CardDetailsDto(), new CardDetailsDto()]
        };

        model.HasCards.Should().BeTrue();
    }
}
