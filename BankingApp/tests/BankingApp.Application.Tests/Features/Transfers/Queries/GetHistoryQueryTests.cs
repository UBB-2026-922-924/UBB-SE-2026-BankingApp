namespace BankingApp.Application.Tests.Features.Transfers.Queries;

using BankingApp.Application.Features.Transfers.Services;
using Contracts.Features.Transfers.Dtos;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;
using Currency = NodaMoney.Currency;
using Money = NodaMoney.Money;

public sealed class GetHistoryQueryTests
{
    private const int TestUserId = 1;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Currency _currency = Currency.FromCode("RON");

    private readonly Mock<ITransferRepository> _transferRepositoryMock = new(MockBehavior.Strict);

    [Fact]
    public async Task GetHistoryAsync_WhenTransferHasReference_MapsReferenceToResponse()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Transfer transfer = CreateTransfer(reference: "Invoice 42");

        _transferRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync([transfer]);

        TransferService service = CreateService();

        // Act
        ErrorOr<List<TransferResponse>> result = await service.GetHistoryAsync(TestUserId, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].Reference.Should().Be("Invoice 42");
        result.Value[0].TransactionRef.Should().BeNull();

        _transferRepositoryMock.Verify(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _transferRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHistoryAsync_WhenTransferHasNoReference_ResponseReferenceIsNull()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Transfer transfer = CreateTransfer(reference: null);

        _transferRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync([transfer]);

        TransferService service = CreateService();

        // Act
        ErrorOr<List<TransferResponse>> result = await service.GetHistoryAsync(TestUserId, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value[0].Reference.Should().BeNull();

        _transferRepositoryMock.Verify(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _transferRepositoryMock.VerifyNoOtherCalls();
    }

    private TransferService CreateService() =>
        new(
            new Mock<IAccountRepository>().Object,
            _transferRepositoryMock.Object,
            new Mock<IBeneficiaryRepository>().Object,
            new Mock<IUnitOfWork>().Object,
            new Mock<ISystemClock>().Object,
            new Mock<IExchangeRateService>().Object,
            NullLogger<TransferService>.Instance);

    private static Transfer CreateTransfer(string? reference) =>
        Transfer.Create(
            TestUserId,
            sourceAccountId: 1,
            recipientName: "Jane Doe",
            recipientIban: Iban.Create("RO12BANK1234567890123456").Value,
            amount: new Money(100m, _currency),
            fee: new Money(1m, _currency),
            reference: reference,
            createdAt: _testNow).Value;
}
