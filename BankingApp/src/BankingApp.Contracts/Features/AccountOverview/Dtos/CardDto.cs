namespace BankingApp.Contracts.Features.AccountOverview.Dtos;

using System.Text.Json.Serialization;
using Domain.Enums;

/// <summary>
///     Data transfer object representing a payment card on the dashboard.
/// </summary>
public class CardDto
{
    /// <summary>
    ///     Gets or sets the masked card number.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name of the account associated with this card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? AccountName { get; set; }

    /// <summary>
    ///     Gets or sets the current balance of the account associated with this card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal? AccountBalance { get; set; }

    /// <summary>
    ///     Gets or sets the name of the cardholder.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string CardholderName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the type of the card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CardType CardType { get; set; }

    /// <summary>
    ///     Gets or sets the brand of the card (e.g. Visa, Mastercard).
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? CardBrand { get; set; }

    /// <summary>
    ///     Gets or sets the expiry date of the card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    ///     Gets or sets the status of the card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CardStatus Status { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether contactless payments are enabled.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsContactlessEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether online payments are enabled.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsOnlineEnabled { get; set; }
}
