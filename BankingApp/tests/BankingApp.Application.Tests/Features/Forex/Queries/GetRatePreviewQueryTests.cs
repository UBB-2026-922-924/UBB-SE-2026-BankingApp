namespace BankingApp.Application.Tests.Features.Forex.Queries;

using BankingApp.Application.Features.Forex.Services;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.Forex.Dtos;
using ErrorOr;
using Shared.Clock;
using Currency = NodaMoney.Currency;

public sealed class GetRatePreviewQueryTests
{
    private const int TestUserId = 1;
    private const decimal SourceAmount = 100m;
    private const decimal ExchangeRate = 1.25m;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IExchangeRateService> _exchangeRateServiceMock = new(MockBehavior.Strict);
    private readonly Mock<ILockedRateCache> _lockedRateCacheMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenCurrencyCodeIsInvalid_ShouldReturnInvalidCurrencyError()
    {
        // Arrange
        ForexService service = CreateService();

        // Act
        ErrorOr<ForexRatePreviewResponse> result = await service.GetRatePreviewAsync(TestUserId, "INVALID", "USD", SourceAmount, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.InvalidCurrency);

        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSourceAndTargetCurrencyAreTheSame_ShouldReturnSameCurrencyError()
    {
        // Arrange
        ForexService service = CreateService();

        // Act
        ErrorOr<ForexRatePreviewResponse> result = await service.GetRatePreviewAsync(TestUserId, "USD", "USD", SourceAmount, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.SameCurrency);

        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldStoreLockedRateInCache()
    {
        // Arrange
        var sourceCurrency = Currency.FromCode("EUR");
        var targetCurrency = Currency.FromCode("USD");

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(sourceCurrency, targetCurrency))
            .Returns((ErrorOr<decimal>)ExchangeRate);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _lockedRateCacheMock
            .Setup(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow));

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexRatePreviewResponse> result = await service.GetRatePreviewAsync(TestUserId, "EUR", "USD", SourceAmount, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();

        _exchangeRateServiceMock.Verify(s => s.GetRate(sourceCurrency, targetCurrency), Times.Once);
        _lockedRateCacheMock.Verify(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldReturnPreviewWithCalculatedTargetAmount()
    {
        // Arrange
        var sourceCurrency = Currency.FromCode("EUR");
        var targetCurrency = Currency.FromCode("USD");

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(sourceCurrency, targetCurrency))
            .Returns((ErrorOr<decimal>)ExchangeRate);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _lockedRateCacheMock
            .Setup(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow));

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexRatePreviewResponse> result = await service.GetRatePreviewAsync(TestUserId, "EUR", "USD", SourceAmount, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.SourceCurrency.Should().Be("EUR");
        result.Value.TargetCurrency.Should().Be("USD");
        result.Value.TargetAmount.Should().Be(125m);
        result.Value.ExchangeRate.Should().Be(ExchangeRate);

        _exchangeRateServiceMock.Verify(s => s.GetRate(sourceCurrency, targetCurrency), Times.Once);
        _lockedRateCacheMock.Verify(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldIncludeCommissionInPreview()
    {
        // Arrange
        var sourceCurrency = Currency.FromCode("EUR");
        var targetCurrency = Currency.FromCode("USD");

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(sourceCurrency, targetCurrency))
            .Returns((ErrorOr<decimal>)ExchangeRate);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _lockedRateCacheMock
            .Setup(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow));

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexRatePreviewResponse> result = await service.GetRatePreviewAsync(TestUserId, "EUR", "USD", SourceAmount, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Commission.Should().Be(0.50m);

        _exchangeRateServiceMock.Verify(s => s.GetRate(sourceCurrency, targetCurrency), Times.Once);
        _lockedRateCacheMock.Verify(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private ForexService CreateService()
    {
        return new ForexService(
            new Mock<IAccountRepository>().Object,
            new Mock<IForexRepository>().Object,
            _lockedRateCacheMock.Object,
            _exchangeRateServiceMock.Object,
            new Mock<Shared.Persistence.IUnitOfWork>().Object,
            _clockMock.Object);
    }
}
