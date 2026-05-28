namespace BankingApp.Infrastructure.Core.Tests.ExchangeRates;

public sealed class ConfigurationExchangeRateServiceTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void GetRate_WhenBaseCurrencyEqualsQuoteCurrency_ShouldReturnOne()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void GetRate_WhenKnownCurrencyPairHasDefaultRate_ShouldReturnDefaultRate()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void GetRate_WhenConfigurationOverridesDefaultRate_ShouldReturnConfiguredRate()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void GetRate_WhenConfiguredRateIsInvalidDecimal_ShouldFallBackToDefaultRate()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void GetRate_WhenCurrencyPairIsUnknownAndNotConfigured_ShouldReturnNotFoundError()
    {
        throw new NotImplementedException();
    }
}
