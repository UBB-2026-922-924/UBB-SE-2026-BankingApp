namespace BankingApp.Application.Tests.Features.UserProfile.Commands;

using Common.Security;
using BankingApp.Application.Features.UserProfile.Services;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class UpdateProfileCommandTests
{
    private const int TestUserId = 1;
    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IUserRepository> _userRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);
    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new();
    private readonly Mock<IHashService> _hashServiceMock = new();

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(TestUserId, cancellationToken)).ReturnsAsync((User?)null);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.UpdateProfileAsync(TestUserId, " Jane Doe ", "+40722123456", new DateTime(1990, 1, 1), " Main Street ", " RO ", " ro ", cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _userRepositoryMock.Verify(repository => repository.GetByIdAsync(TestUserId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUpdateUserProfile()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        SetupValid(user, cancellationToken);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.UpdateProfileAsync(TestUserId, " Jane Doe ", "+40722123456", new DateTime(1990, 1, 1), " Main Street ", " RO ", " ro ", cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        user.FullName.Should().Be("Jane Doe");
        user.PhoneNumber.Should().Be("+40722123456");
        user.Address.Should().Be("Main Street");
        user.Nationality.Should().Be("RO");
        user.PreferredLanguage.Should().Be("ro");
        user.UpdatedAt.Should().Be(_testNow);
        VerifyValid(user, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        SetupValid(user, cancellationToken);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.UpdateProfileAsync(TestUserId, " Jane Doe ", "+40722123456", new DateTime(1990, 1, 1), " Main Street ", " RO ", " ro ", cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        VerifyValid(user, cancellationToken);
    }

    private UserProfileService CreateService() =>
        new(
            _userRepositoryMock.Object,
            _identityRepositoryMock.Object,
            _hashServiceMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            NullLogger<UserProfileService>.Instance);

    private void SetupValid(User user, CancellationToken cancellationToken)
    {
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(TestUserId, cancellationToken)).ReturnsAsync(user);
        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);
        _userRepositoryMock.Setup(repository => repository.UpdateAsync(user, cancellationToken)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);
    }

    private void VerifyValid(User user, CancellationToken cancellationToken)
    {
        _userRepositoryMock.Verify(repository => repository.GetByIdAsync(TestUserId, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _userRepositoryMock.Verify(repository => repository.UpdateAsync(user, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    private void VerifyNoOtherCalls()
    {
        _userRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private static User CreateUser() => User.Register(Email.Create("test@example.com").Value, "Test User", _testNow);
}
