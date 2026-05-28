namespace BankingApp.Application.Tests.Features.Beneficiaries.Commands;

using BankingApp.Application.Features.Beneficiaries.Services;
using Domain.Aggregates.BeneficiaryAggregate;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.Beneficiaries.Dtos;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class CreateBeneficiaryCommandTests
{
    private const int TestUserId = 1;
    private const string ValidIban = "RO12BANK1234567890123456";

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenIbanIsInvalid_ShouldReturnInvalidIbanErrorAndNotPersist()
    {
        // Arrange
        BeneficiaryService service = CreateService();

        // Act
        ErrorOr<BeneficiaryDto> result = await service.CreateAsync(TestUserId, "John Doe", "invalid-iban", "Bank", TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.InvalidIban);

        _beneficiaryRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenBeneficiaryWithSameIbanAlreadyExistsIgnoringCase_ShouldReturnDuplicateErrorAndNotPersist()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Beneficiary existingBeneficiary = CreateBeneficiary(ValidIban.ToLowerInvariant());

        _beneficiaryRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync([existingBeneficiary]);

        BeneficiaryService service = CreateService();

        // Act
        ErrorOr<BeneficiaryDto> result = await service.CreateAsync(TestUserId, "John Doe", ValidIban, "Bank", cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BeneficiaryErrors.Duplicate);

        _beneficiaryRepositoryMock.Verify(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _beneficiaryRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldCreateTrimmedBeneficiaryAndSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Beneficiary? persistedBeneficiary = null;

        _beneficiaryRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync([]);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _beneficiaryRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Beneficiary>(), cancellationToken))
            .Callback<Beneficiary, CancellationToken>((beneficiary, _) => persistedBeneficiary = beneficiary)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        BeneficiaryService service = CreateService();

        // Act
        ErrorOr<BeneficiaryDto> result = await service.CreateAsync(TestUserId, "  John Doe  ", ValidIban, "Bank", cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        persistedBeneficiary.Should().NotBeNull();
        persistedBeneficiary!.UserId.Should().Be(TestUserId);
        persistedBeneficiary.Name.Should().Be("John Doe");
        persistedBeneficiary.Iban.Value.Should().Be(ValidIban);
        persistedBeneficiary.BankName.Should().Be("Bank");
        persistedBeneficiary.CreatedAt.Should().Be(_testNow);

        result.Value.UserId.Should().Be(TestUserId);
        result.Value.Name.Should().Be("John Doe");
        result.Value.Iban.Should().Be(ValidIban);
        result.Value.BankName.Should().Be("Bank");

        _beneficiaryRepositoryMock.Verify(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _beneficiaryRepositoryMock.Verify(repository => repository.AddAsync(persistedBeneficiary, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _beneficiaryRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private BeneficiaryService CreateService()
    {
        return new BeneficiaryService(
            _beneficiaryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            NullLogger<BeneficiaryService>.Instance);
    }

    private static Beneficiary CreateBeneficiary(string iban = ValidIban)
    {
        return Beneficiary.Create(TestUserId, "Jane Doe", Iban.Create(iban).Value, null, _testNow);
    }
}
