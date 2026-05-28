namespace BankingApp.Contracts.Features.Forex;

using System.Collections.Generic;

/// <summary>
///     Shared constants for the Forex feature.
/// </summary>
public static class ForexConstants
{
    /// <summary>
    ///     The list of ISO 4217 currency codes supported for exchange.
    /// </summary>
    public static IReadOnlyList<string> SupportedCurrencies { get; } =
        ["USD", "EUR", "GBP", "RON", "CHF", "JPY", "CAD", "AUD"];
}
