namespace BankingApp.Application.Tests.Features.Billers.Commands;

using BankingApp.Application.Features.Billers.Services;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Persistence;

public sealed class DeleteSavedBillerCommandTests
{
    private const int TestUserId = 1;
    private const int OtherUserId = 2;
    private const int TestSavedBillerId = 100;
    private const int TestBillerId = 200;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISavedBillerRepository> _savedBillerRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenSavedBillerNotFound_ShouldReturnNotFoundErrorAndNotPersist()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _savedBillerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestSavedBillerId, cancellationToken))
            .ReturnsAsync((SavedBiller?)null);

        BillerService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.DeleteSavedBillerAsync(TestUserId, TestSavedBillerId, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillerErrors.SavedBillerNotFound);

        _savedBillerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestSavedBillerId, cancellationToken), Times.Once);
        _savedBillerRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSavedBillerBelongsToDifferentUser_ShouldReturnNotFoundErrorAndNotPersist()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        SavedBiller otherUserSavedBiller = CreateSavedBiller(OtherUserId);

        _savedBillerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestSavedBillerId, cancellationToken))
            .ReturnsAsync(otherUserSavedBiller);

        BillerService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.DeleteSavedBillerAsync(TestUserId, TestSavedBillerId, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillerErrors.SavedBillerNotFound);

        _savedBillerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestSavedBillerId, cancellationToken), Times.Once);
        _savedBillerRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldDeleteSavedBillerAndSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        SavedBiller savedBiller = CreateSavedBiller(TestUserId);

        _savedBillerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestSavedBillerId, cancellationToken))
            .ReturnsAsync(savedBiller);

        _savedBillerRepositoryMock
            .Setup(repository => repository.DeleteAsync(savedBiller, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        BillerService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.DeleteSavedBillerAsync(TestUserId, TestSavedBillerId, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        _savedBillerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestSavedBillerId, cancellationToken), Times.Once);
        _savedBillerRepositoryMock.Verify(repository => repository.DeleteAsync(savedBiller, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _savedBillerRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    private BillerService CreateService()
    {
        return new BillerService(
            new Mock<IBillerRepository>().Object,
            _savedBillerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new Mock<Shared.Clock.ISystemClock>().Object);
    }

    private static SavedBiller CreateSavedBiller(int userId)
    {
        return SavedBiller.Create(userId, TestBillerId, "Power", "ACC-123", _testNow);
    }
}
