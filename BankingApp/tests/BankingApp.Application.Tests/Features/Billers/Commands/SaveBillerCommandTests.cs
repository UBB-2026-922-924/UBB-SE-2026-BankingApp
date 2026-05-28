namespace BankingApp.Application.Tests.Features.Billers.Commands;

using BankingApp.Application.Features.Billers.Services;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.Billers.Dtos;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;

public sealed class SaveBillerCommandTests
{
    private const int TestUserId = 1;
    private const int TestBillerId = 100;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IBillerRepository> _billerRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<ISavedBillerRepository> _savedBillerRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenBillerNotFound_ShouldReturnBillerNotFoundErrorAndNotPersist()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _billerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBillerId, cancellationToken))
            .ReturnsAsync((Biller?)null);

        BillerService service = CreateService();

        // Act
        ErrorOr<SavedBillerDto> result = await service.SaveBillerAsync(TestUserId, TestBillerId, "Power", "ACC-123", cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillerErrors.BillerNotFound);

        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBillerId, cancellationToken), Times.Once);
        _billerRepositoryMock.VerifyNoOtherCalls();
        _savedBillerRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenBillerAlreadySaved_ShouldReturnBillerAlreadySavedErrorAndNotPersist()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Biller biller = CreateBiller();
        SavedBiller savedBiller = CreateSavedBiller(TestUserId, TestBillerId);

        _billerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBillerId, cancellationToken))
            .ReturnsAsync(biller);

        _savedBillerRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync([savedBiller]);

        BillerService service = CreateService();

        // Act
        ErrorOr<SavedBillerDto> result = await service.SaveBillerAsync(TestUserId, TestBillerId, "Power", "ACC-123", cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillerErrors.BillerAlreadySaved);

        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBillerId, cancellationToken), Times.Once);
        _savedBillerRepositoryMock.Verify(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _billerRepositoryMock.VerifyNoOtherCalls();
        _savedBillerRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCreateAndPersistSavedBiller()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Biller biller = CreateBiller();
        SavedBiller? persistedSavedBiller = null;

        _billerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBillerId, cancellationToken))
            .ReturnsAsync(biller);

        _savedBillerRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync([]);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _savedBillerRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<SavedBiller>(), cancellationToken))
            .Callback<SavedBiller, CancellationToken>((savedBiller, _) => persistedSavedBiller = savedBiller)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        BillerService service = CreateService();

        // Act
        ErrorOr<SavedBillerDto> result = await service.SaveBillerAsync(TestUserId, TestBillerId, "  Power  ", "  ACC-123  ", cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        persistedSavedBiller.Should().NotBeNull();
        persistedSavedBiller!.UserId.Should().Be(TestUserId);
        persistedSavedBiller.BillerId.Should().Be(TestBillerId);
        persistedSavedBiller.Nickname.Should().Be("  Power  ");
        persistedSavedBiller.DefaultReference.Should().Be("  ACC-123  ");
        persistedSavedBiller.CreatedAt.Should().Be(_testNow);

        result.Value.UserId.Should().Be(TestUserId);
        result.Value.BillerId.Should().Be(TestBillerId);
        result.Value.BillerName.Should().Be(biller.Name);
        result.Value.BillerCategory.Should().Be(biller.Category.ToString());
        result.Value.LogoUrl.Should().Be(biller.LogoUrl);
        result.Value.Nickname.Should().Be("  Power  ");
        result.Value.DefaultReference.Should().Be("  ACC-123  ");
        result.Value.CreatedAt.Should().Be(_testNow);
        result.Value.Biller.Should().NotBeNull();
        result.Value.Biller!.Id.Should().Be(biller.Id);
        result.Value.Biller.Name.Should().Be(biller.Name);
        result.Value.Biller.Category.Should().Be(biller.Category.ToString());
        result.Value.Biller.LogoUrl.Should().Be(biller.LogoUrl);
        result.Value.Biller.IsActive.Should().Be(biller.IsActive);

        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBillerId, cancellationToken), Times.Once);
        _savedBillerRepositoryMock.Verify(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _savedBillerRepositoryMock.Verify(repository => repository.AddAsync(persistedSavedBiller, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _billerRepositoryMock.VerifyNoOtherCalls();
        _savedBillerRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Biller biller = CreateBiller();

        _billerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBillerId, cancellationToken))
            .ReturnsAsync(biller);

        _savedBillerRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync([]);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _savedBillerRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<SavedBiller>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        BillerService service = CreateService();

        // Act
        ErrorOr<SavedBillerDto> result = await service.SaveBillerAsync(TestUserId, TestBillerId, null, null, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();

        _billerRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBillerId, cancellationToken), Times.Once);
        _savedBillerRepositoryMock.Verify(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _savedBillerRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<SavedBiller>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _billerRepositoryMock.VerifyNoOtherCalls();
        _savedBillerRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private BillerService CreateService()
    {
        return new BillerService(
            _billerRepositoryMock.Object,
            _savedBillerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object);
    }

    private static Biller CreateBiller()
    {
        return new Biller
        {
            Id = TestBillerId,
            Name = "Electric Company",
            Category = BillerCategory.Utilities,
            LogoUrl = "https://example.test/logo.png",
            IsActive = true
        };
    }

    private static SavedBiller CreateSavedBiller(int userId, int billerId)
    {
        return SavedBiller.Create(userId, billerId, "Power", "ACC-123", _testNow);
    }
}
