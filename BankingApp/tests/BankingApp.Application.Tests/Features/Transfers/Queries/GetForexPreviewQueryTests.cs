namespace BankingApp.Application.Tests.Features.Transfers.Queries;

using BankingApp.Application;
using BankingApp.Application.Features.Transfers.Services;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.Transfers.Dtos;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Currency = NodaMoney.Currency;

public sealed class GetForexPreviewQueryTests
{
    private readonly Mock<IExchangeRateService> _exchangeRateServiceMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenCurrencyCodeIsInvalid_ShouldReturnInvalidCurrencyError()
    {
        // Arrange
        TransferService service = CreateService();

        // Act
        ErrorOr<TransferForexPreviewResponse> result = await service.GetFxPreviewAsync("INVALID", "USD", 100m, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.InvalidCurrency);

        _exchangeRateServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSourceAndTargetCurrencyAreTheSame_ShouldReturnSameCurrencyError()
    {
        // Arrange
        var currency = Currency.FromCode("USD");

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(currency, currency))
            .Returns((ErrorOr<decimal>)ForexErrors.SameCurrency);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferForexPreviewResponse> result = await service.GetFxPreviewAsync("USD", "USD", 100m, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.SameCurrency);

        _exchangeRateServiceMock.Verify(s => s.GetRate(currency, currency), Times.Once);
        _exchangeRateServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldReturnPreviewWithCalculatedConvertedAmount()
    {
        // Arrange
        var source = Currency.FromCode("EUR");
        var target = Currency.FromCode("USD");

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(source, target))
            .Returns((ErrorOr<decimal>)1.25m);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferForexPreviewResponse> result = await service.GetFxPreviewAsync("EUR", "USD", 100m, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ExchangeRate.Should().Be(1.25m);
        result.Value.ConvertedAmount.Should().Be(125m);

        _exchangeRateServiceMock.Verify(s => s.GetRate(source, target), Times.Once);
        _exchangeRateServiceMock.VerifyNoOtherCalls();
    }

    private TransferService CreateService()
    {
        return new TransferService(
            new Mock<IAccountRepository>().Object,
            new Mock<ITransferRepository>().Object,
            new Mock<IBeneficiaryRepository>().Object,
            new Mock<Shared.Persistence.IUnitOfWork>().Object,
            new Mock<Shared.Clock.ISystemClock>().Object,
            _exchangeRateServiceMock.Object,
            NullLogger<TransferService>.Instance);
    }
}
