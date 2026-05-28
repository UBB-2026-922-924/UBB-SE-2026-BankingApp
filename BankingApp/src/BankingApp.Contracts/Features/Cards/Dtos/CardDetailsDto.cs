namespace BankingApp.Contracts.Features.Cards.Dtos;

using System;
using System.Globalization;
using System.Text.Json.Serialization;
using Domain.Enums;

/// <summary>Data transfer object for a card in the card management view.</summary>
public sealed class CardDetailsDto
{
    /// <summary>Gets or sets the card identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the masked card number (last 4 digits visible).</summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the full unmasked card number.</summary>
    public string FullCardNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the card security code (CVV).</summary>
    public string SecurityCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the cardholder.</summary>
    public string CardholderName { get; set; } = string.Empty;

    /// <summary>Gets or sets the expiry date of the card.</summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>Gets or sets the card type.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CardType CardType { get; set; }

    /// <summary>Gets or sets the card brand (e.g. Visa, Mastercard).</summary>
    public string? CardBrand { get; set; }

    /// <summary>Gets or sets the card status.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CardStatus Status { get; set; }

    /// <summary>Gets or sets a value indicating whether contactless payments are enabled.</summary>
    public bool IsContactlessEnabled { get; set; }

    /// <summary>Gets or sets a value indicating whether online payments are enabled.</summary>
    public bool IsOnlineEnabled { get; set; }

    /// <summary>Gets or sets the display name of the associated account.</summary>
    public string? AccountName { get; set; }

    /// <summary>Gets or sets the IBAN of the associated account.</summary>
    public string AccountIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the associated account.</summary>
    public int AccountId { get; set; }

    /// <summary>Gets or sets the current balance of the associated account.</summary>
    public decimal AccountBalance { get; set; }

    /// <summary>Gets or sets the ISO currency code of the associated account (e.g. "USD").</summary>
    public string AccountCurrency { get; set; } = string.Empty;

    /// <summary>Gets the expiry date formatted as MM/YY for display.</summary>
    public string ExpiryDisplay => ExpiryDate.ToString("MM/yy", CultureInfo.InvariantCulture);

    /// <summary>Gets the account balance formatted with its currency symbol for display.</summary>
    public string BalanceDisplay => string.IsNullOrEmpty(AccountCurrency)
        ? AccountBalance.ToString("N2", CultureInfo.InvariantCulture)
        : $"{AccountBalance.ToString("N2", CultureInfo.InvariantCulture)} {AccountCurrency}";

    /// <summary>Gets the card brand or falls back to the card type string.</summary>
    public string BrandDisplay => string.IsNullOrWhiteSpace(CardBrand) ? CardType.ToString() : CardBrand;

    /// <summary>Gets whether this card can be frozen (only Active cards).</summary>
    public bool CanFreeze => Status == CardStatus.Active;

    /// <summary>Gets whether this card can be unfrozen (only Frozen cards).</summary>
    public bool CanUnfreeze => Status == CardStatus.Frozen;

    /// <summary>Gets whether this card can be cancelled (not already cancelled).</summary>
    public bool CanCancel => Status != CardStatus.Cancelled;

    /// <summary>Gets the opacity to apply to a cancelled card (dimmed) versus an active one (full).</summary>
    public double DisplayOpacity => Status == CardStatus.Cancelled ? 0.45 : 1.0;
}
