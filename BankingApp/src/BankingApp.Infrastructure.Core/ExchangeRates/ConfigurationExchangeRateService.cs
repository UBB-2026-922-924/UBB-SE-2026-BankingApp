namespace BankingApp.Infrastructure.Core.ExchangeRates;

using System.Globalization;
using Application;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Currency = NodaMoney.Currency;

// TODO: replace this with an API (ex. https://www.exchangerate-api.com/)
public sealed class ConfigurationExchangeRateService(IConfiguration configuration) : IExchangeRateService
{
    private static readonly Dictionary<string, decimal> _defaultRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["EUR:USD"] = 1.08m,
        ["USD:EUR"] = 0.93m,
        ["EUR:RON"] = 4.97m,
        ["RON:EUR"] = 0.20m,
        ["USD:RON"] = 4.61m,
        ["RON:USD"] = 0.22m,
        ["GBP:EUR"] = 1.17m,
        ["EUR:GBP"] = 0.85m,
        ["GBP:USD"] = 1.27m,
        ["USD:GBP"] = 0.79m
    };

    public ErrorOr<decimal> GetRate(Currency baseCurrency, Currency quoteCurrency)
    {
        if (baseCurrency == quoteCurrency)
        {
            return 1m;
        }

        string key = BuildKey(baseCurrency, quoteCurrency);
        string? configuredValue = configuration[$"ExchangeRates:{key}"];
        if (configuredValue is not null &&
            decimal.TryParse(configuredValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal configuredRate) &&
            configuredRate > 0)
        {
            return configuredRate;
        }

        if (_defaultRates.TryGetValue(key, out decimal defaultRate))
        {
            return defaultRate;
        }

        return Error.NotFound("exchange_rate.not_found", $"No exchange rate configured for {key}.");
    }

    private static string BuildKey(Currency baseCurrency, Currency quoteCurrency) =>
        $"{baseCurrency.Code}:{quoteCurrency.Code}";
}
