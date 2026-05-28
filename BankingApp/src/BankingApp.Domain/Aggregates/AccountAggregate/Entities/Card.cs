namespace BankingApp.Domain.Aggregates.AccountAggregate.Entities;

using Common.Primitives;
using Enums;

public sealed class Card : Entity<int>
{
    private const int VisibleSuffixLength = 4;
    private const string MaskedPrefix = "**** **** ****";
    private const string FullyMasked = "**** **** **** ****";

    private Card()
    {
    }

    public int AccountId { get; private set; }

    public int UserId { get; private set; }

    public string CardNumber { get; private set; } = string.Empty;

    public string CardholderName { get; private set; } = string.Empty;

    public DateTime ExpiryDate { get; private set; }

    public string Cvv { get; private set; } = string.Empty;

    public CardType CardType { get; private set; }

    public string? CardBrand { get; private set; }

    public CardStatus Status { get; private set; } = CardStatus.Active;

    public decimal? DailyTransactionLimit { get; private set; }

    public decimal? MonthlySpendingCap { get; private set; }

    public decimal? AtmWithdrawalLimit { get; private set; }

    public decimal? ContactlessLimit { get; private set; }

    public bool IsContactlessEnabled { get; private set; } = true;

    public bool IsOnlineEnabled { get; private set; } = true;

    public int SortOrder { get; private set; }

    public DateTime? CancelledAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Card Create(
        int accountId,
        int userId,
        string cardNumber,
        string cardholderName,
        DateTime expiryDate,
        string cvv,
        CardType cardType,
        string? cardBrand,
        DateTime createdAt)
    {
        return new Card
        {
            AccountId = accountId,
            UserId = userId,
            CardNumber = cardNumber,
            CardholderName = cardholderName,
            ExpiryDate = expiryDate,
            Cvv = cvv,
            CardType = cardType,
            CardBrand = cardBrand,
            CreatedAt = createdAt
        };
    }

    public string GetMaskedNumber()
    {
        if (string.IsNullOrWhiteSpace(CardNumber) || CardNumber.Length < VisibleSuffixLength)
        {
            return FullyMasked;
        }

        return $"{MaskedPrefix} {CardNumber[^VisibleSuffixLength..]}";
    }

    public bool IsExpired() => ExpiryDate < DateTime.UtcNow;

    public bool IsActive() => Status == CardStatus.Active;

    public void Cancel(DateTime cancelledAt)
    {
        Status = CardStatus.Cancelled;
        CancelledAt = cancelledAt;
    }

    public void Freeze()
    {
        Status = CardStatus.Frozen;
    }

    public void Unfreeze()
    {
        Status = CardStatus.Active;
    }
}
