namespace BankingApp.Domain.Tests.Aggregates.AccountAggregate.Entities;

using BankingApp.Domain.Aggregates.AccountAggregate.Entities;
using Enums;

public sealed class CardTests
{
    private static Card CreateCard(string cardNumber = "1234567890123456", DateTime? expiryDate = null)
    {
        return Card.Create(
            accountId: 1,
            userId: 1,
            cardNumber: cardNumber,
            cardholderName: "John Doe",
            expiryDate: expiryDate ?? DateTime.UtcNow.AddYears(2),
            cvv: "123",
            cardType: CardType.Debit,
            cardBrand: "Visa",
            createdAt: DateTime.UtcNow);
    }

    [Fact]
    public void GetMaskedNumber_WhenCardNumberIsLongEnough_ShouldShowLastFourDigits()
    {
        Card card = CreateCard("1234567890123456");

        string masked = card.GetMaskedNumber();

        Assert.Equal("**** **** **** 3456", masked);
    }

    [Fact]
    public void GetMaskedNumber_WhenCardNumberIsTooShort_ShouldReturnFullyMasked()
    {
        Card card = CreateCard("123");

        string masked = card.GetMaskedNumber();

        Assert.Equal("**** **** **** ****", masked);
    }

    [Fact]
    public void IsExpired_WhenExpiryDateIsInThePast_ShouldReturnTrue()
    {
        Card card = CreateCard(expiryDate: DateTime.UtcNow.AddDays(-1));

        Assert.True(card.IsExpired());
    }

    [Fact]
    public void IsExpired_WhenExpiryDateIsInTheFuture_ShouldReturnFalse()
    {
        Card card = CreateCard(expiryDate: DateTime.UtcNow.AddYears(1));

        Assert.False(card.IsExpired());
    }

    [Fact]
    public void Cancel_WhenCalled_ShouldSetStatusToCancelled()
    {
        Card card = CreateCard();
        DateTime cancelledAt = DateTime.UtcNow;

        card.Cancel(cancelledAt);

        Assert.Equal(CardStatus.Cancelled, card.Status);
    }

    [Fact]
    public void Cancel_WhenCalled_ShouldSetCancelledAt()
    {
        Card card = CreateCard();
        var cancelledAt = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        card.Cancel(cancelledAt);

        Assert.Equal(cancelledAt, card.CancelledAt);
    }

    [Fact]
    public void Freeze_WhenCalled_ShouldSetStatusToFrozen()
    {
        Card card = CreateCard();

        card.Freeze();

        Assert.Equal(CardStatus.Frozen, card.Status);
    }

    [Fact]
    public void Unfreeze_WhenCalledOnFrozenCard_ShouldSetStatusToActive()
    {
        Card card = CreateCard();
        card.Freeze();

        card.Unfreeze();

        Assert.Equal(CardStatus.Active, card.Status);
    }
}

