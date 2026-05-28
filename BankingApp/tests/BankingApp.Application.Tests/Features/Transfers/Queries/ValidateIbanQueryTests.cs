namespace BankingApp.Application.Tests.Features.Transfers.Queries;

using BankingApp.Application.Features.Transfers.Services;
using Contracts.Features.Transfers.Dtos;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class ValidateIbanQueryTests
{
    [Fact]
    public async Task Handle_WhenIbanIsValid_ShouldReturnValidResponse()
    {
        // Arrange
        TransferService service = CreateService();

        // Act
        ErrorOr<TransferIbanValidationResponse> result = await service.ValidateIbanAsync("RO12BANK1234567890123456", TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsValid.Should().BeTrue();
        result.Value.BankName.Should().Be("Romanian Bank");
    }

    [Fact]
    public async Task Handle_WhenIbanIsInvalid_ShouldReturnInvalidResponse()
    {
        // Arrange
        TransferService service = CreateService();

        // Act
        ErrorOr<TransferIbanValidationResponse> result = await service.ValidateIbanAsync("invalid-iban", TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsValid.Should().BeFalse();
        result.Value.BankName.Should().BeEmpty();
    }

    private static TransferService CreateService()
    {
        return new TransferService(
            new Mock<IAccountRepository>().Object,
            new Mock<ITransferRepository>().Object,
            new Mock<IBeneficiaryRepository>().Object,
            new Mock<Shared.Persistence.IUnitOfWork>().Object,
            new Mock<Shared.Clock.ISystemClock>().Object,
            new Mock<BankingApp.Application.IExchangeRateService>().Object,
            NullLogger<TransferService>.Instance);
    }
}
