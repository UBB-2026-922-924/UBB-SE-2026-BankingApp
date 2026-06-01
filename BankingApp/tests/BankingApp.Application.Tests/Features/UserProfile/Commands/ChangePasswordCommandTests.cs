namespace BankingApp.Application.Tests.Features.UserProfile.Commands;

using Common.Security;
using BankingApp.Application.Features.UserProfile.Services;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class ChangePasswordCommandTests
{
    private const int TestUserId = 1;
    private const string CurrentPassword = "OldPassword123!";
    private const string NewPassword = "NewPassword123!";
    private const string OldHash = "old-hash";
    private const string NewHash = "new-hash";

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IUserRepository> _userRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IHashService> _hashServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new();

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(TestUserId, cancellationToken)).ReturnsAsync((User?)null);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.ChangePasswordAsync(TestUserId, CurrentPassword, NewPassword, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _userRepositoryMock.Verify(repository => repository.GetByIdAsync(TestUserId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(TestUserId, cancellationToken)).ReturnsAsync(user);
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync((IdentityAccount?)null);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.ChangePasswordAsync(TestUserId, CurrentPassword, NewPassword, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _userRepositoryMock.Verify(repository => repository.GetByIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordVerificationFails_ShouldReturnIncorrectPasswordError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap(OldHash));
        SetupUserAndIdentity(user, identity, cancellationToken);
        _hashServiceMock.Setup(service => service.Verify(CurrentPassword, OldHash)).Returns((ErrorOr<bool>)false);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.ChangePasswordAsync(TestUserId, CurrentPassword, NewPassword, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ProfileErrors.IncorrectPassword);
        _userRepositoryMock.Verify(repository => repository.GetByIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _hashServiceMock.Verify(service => service.Verify(CurrentPassword, OldHash), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenPasswordHashingFails_ShouldReturnHashFailureError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap(OldHash));
        var hashError = Error.Failure("hash_failure", "Hash failed.");
        SetupUserAndIdentity(user, identity, cancellationToken);
        _hashServiceMock.Setup(service => service.Verify(CurrentPassword, OldHash)).Returns((ErrorOr<bool>)true);
        _hashServiceMock.Setup(service => service.GetHash(NewPassword)).Returns((ErrorOr<string>)hashError);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.ChangePasswordAsync(TestUserId, CurrentPassword, NewPassword, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(hashError);
        _userRepositoryMock.Verify(repository => repository.GetByIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _hashServiceMock.Verify(service => service.Verify(CurrentPassword, OldHash), Times.Once);
        _hashServiceMock.Verify(service => service.GetHash(NewPassword), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUpdatePasswordHash()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap(OldHash));
        SetupValid(user, identity, cancellationToken);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.ChangePasswordAsync(TestUserId, CurrentPassword, NewPassword, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        identity.PasswordHash.Should().NotBeNull();
        identity.PasswordHash!.Value.Should().Be(NewHash);
        VerifyValid(identity, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap(OldHash));
        SetupValid(user, identity, cancellationToken);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.ChangePasswordAsync(TestUserId, CurrentPassword, NewPassword, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        VerifyValid(identity, cancellationToken);
    }

    private UserProfileService CreateService() =>
        new(
            _userRepositoryMock.Object,
            _identityRepositoryMock.Object,
            _hashServiceMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            NullLogger<UserProfileService>.Instance);

    private void SetupUserAndIdentity(User user, IdentityAccount identity, CancellationToken cancellationToken)
    {
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(TestUserId, cancellationToken)).ReturnsAsync(user);
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync(identity);
    }

    private void SetupValid(User user, IdentityAccount identity, CancellationToken cancellationToken)
    {
        SetupUserAndIdentity(user, identity, cancellationToken);
        _hashServiceMock.Setup(service => service.Verify(CurrentPassword, OldHash)).Returns((ErrorOr<bool>)true);
        _hashServiceMock.Setup(service => service.GetHash(NewPassword)).Returns((ErrorOr<string>)NewHash);
        _identityRepositoryMock.Setup(repository => repository.UpdateAsync(identity, cancellationToken)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);
    }

    private void VerifyValid(IdentityAccount identity, CancellationToken cancellationToken)
    {
        _userRepositoryMock.Verify(repository => repository.GetByIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _hashServiceMock.Verify(service => service.Verify(CurrentPassword, OldHash), Times.Once);
        _hashServiceMock.Verify(service => service.GetHash(NewPassword), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.UpdateAsync(identity, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    private void VerifyNoOtherCalls()
    {
        _userRepositoryMock.VerifyNoOtherCalls();
        _identityRepositoryMock.VerifyNoOtherCalls();
        _hashServiceMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    private static User CreateUser()
    {
        var user = User.Register(Email.Create("test@example.com").Value, "Test User", _testNow);
        typeof(User).BaseType!.GetProperty("Id")!.SetValue(user, TestUserId);
        return user;
    }
}
