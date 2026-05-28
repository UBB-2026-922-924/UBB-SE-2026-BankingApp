namespace BankingApp.Web.Models.Dashboard;

using System.Globalization;
using BankingApp.Contracts.Features.AccountOverview.Dtos;
using BankingApp.Domain.Enums;

/// <summary>Model for a single payment card displayed on the dashboard.</summary>
public class DashboardCardModel
{
    private const int CardNumberVisibleSuffixLength = 4;
    private const string FullyMaskedCardNumber = "**** **** **** ****";
    private const string CardNumberMaskPrefix = "**** **** ****";

    /// <summary>Gets the card brand / type display text (e.g. "Visa", "Debit").</summary>
    public string BrandDisplay { get; }

    /// <summary>Gets the masked card number (e.g. "**** **** **** 1234").</summary>
    public string MaskedNumber { get; }

    /// <summary>Gets the cardholder name in upper-case, or "CARD HOLDER" if blank.</summary>
    public string CardholderDisplay { get; }

    /// <summary>Gets the expiry date formatted as MM/yy.</summary>
    public string ExpiryDisplay { get; }

    /// <summary>Gets the card status.</summary>
    public CardStatus Status { get; }

    /// <summary>Gets the Bootstrap badge CSS class for the card status.</summary>
    public string StatusBadgeClass { get; }

    /// <summary>Gets a value indicating whether contactless payments are enabled.</summary>
    public bool IsContactlessEnabled { get; }

    /// <summary>Gets a value indicating whether online payments are enabled.</summary>
    public bool IsOnlineEnabled { get; }

    /// <summary>Gets the multi-line detail string shown when the user inspects the card.</summary>
    public string Details { get; }

    /// <summary>Initialises a new <see cref="DashboardCardModel"/> from a <see cref="CardDto"/>.</summary>
    public DashboardCardModel(CardDto dto)
    {
        BrandDisplay = string.IsNullOrWhiteSpace(dto.CardBrand)
            ? dto.CardType.ToString()
            : dto.CardBrand;

        MaskedNumber = MaskCardNumber(dto.CardNumber);

        CardholderDisplay = string.IsNullOrWhiteSpace(dto.CardholderName)
            ? "CARD HOLDER"
            : dto.CardholderName.ToUpperInvariant();

        ExpiryDisplay = dto.ExpiryDate.ToString("MM/yy", CultureInfo.InvariantCulture);

        Status = dto.Status;

        StatusBadgeClass = dto.Status switch
        {
            CardStatus.Active    => "bg-success",
            CardStatus.Frozen    => "bg-warning text-dark",
            CardStatus.Expired   => "bg-danger",
            CardStatus.Cancelled => "bg-secondary",
            _                    => "bg-secondary"
        };

        IsContactlessEnabled = dto.IsContactlessEnabled;
        IsOnlineEnabled      = dto.IsOnlineEnabled;

        Details =
            $"Card Type:       {dto.CardType}\n" +
            $"Card Brand:      {dto.CardBrand ?? "Mastercard"}\n" +
            $"Card Number:     {MaskCardNumber(dto.CardNumber)}\n" +
            $"Cardholder:      {dto.CardholderName}\n" +
            $"Expiry Date:     {dto.ExpiryDate:MM/yy}\n" +
            $"Status:          {dto.Status}\n" +
            $"Contactless:     {(dto.IsContactlessEnabled ? "Enabled" : "Disabled")}\n" +
            $"Online Payments: {(dto.IsOnlineEnabled ? "Enabled" : "Disabled")}";
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return FullyMaskedCardNumber;
        }

        return cardNumber.Length >= CardNumberVisibleSuffixLength
            ? $"{CardNumberMaskPrefix} {cardNumber[^CardNumberVisibleSuffixLength..]}"
            : FullyMaskedCardNumber;
    }
}
