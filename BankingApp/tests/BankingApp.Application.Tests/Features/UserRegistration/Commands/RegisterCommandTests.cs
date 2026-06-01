namespace BankingApp.Application.Tests.Features.UserRegistration.Commands;

using Common.Security;
using BankingApp.Application.Features.Authentication.Services;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class RegisterCommandTests
{
    private const string TestEmail = "test@example.com";
    private const string TestPassword = "Password123!";
    private const string PasswordHash = "password-hash";
    private const string FullName = "Test User";

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IUserRepository> _userRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IHashService> _hashServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);
    private readonly Mock<IJsonWebTokenService> _jwtServiceMock = new();

    [Fact]
    public async Task Handle_WhenEmailIsAlreadyRegistered_ShouldReturnEmailAlreadyRegisteredError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var existingUser = User.Register(Email.Create(TestEmail).Value, FullName, _testNow);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken))
            .ReturnsAsync(existingUser);

        AuthService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.RegisterAsync(TestEmail, TestPassword, FullName, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthErrors.EmailAlreadyRegistered);

        _userRepositoryMock.Verify(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenPasswordHashingFails_ShouldReturnHashFailureError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var hashError = Error.Failure("hash_failure", "Hash failed.");

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken))
            .ReturnsAsync((User?)null);

        _hashServiceMock
            .Setup(service => service.GetHash(TestPassword))
            .Returns((ErrorOr<string>)hashError);

        AuthService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.RegisterAsync(TestEmail, TestPassword, FullName, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(hashError);

        _userRepositoryMock.Verify(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken), Times.Once);
        _hashServiceMock.Verify(service => service.GetHash(TestPassword), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCreateUserAndIdentityAccount()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User? persistedUser = null;
        IdentityAccount? persistedIdentity = null;
        SetupValidFlow(cancellationToken, user => persistedUser = user, identity => persistedIdentity = identity);

        AuthService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.RegisterAsync(TestEmail, TestPassword, "  Test User  ", cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        persistedUser.Should().NotBeNull();
        persistedUser!.Email.Value.Should().Be(TestEmail);
        persistedUser.FullName.Should().Be(FullName);
        persistedUser.CreatedAt.Should().Be(_testNow);
        persistedUser.UpdatedAt.Should().Be(_testNow);
        persistedIdentity.Should().NotBeNull();
        persistedIdentity!.UserId.Should().Be(persistedUser.Id);
        persistedIdentity.PasswordHash.Should().NotBeNull();
        persistedIdentity.PasswordHash!.Value.Should().Be(PasswordHash);

        VerifyValidFlow(persistedUser, persistedIdentity, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        SetupValidFlow(cancellationToken);
        AuthService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.RegisterAsync(TestEmail, TestPassword, FullName, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        _userRepositoryMock.Verify(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken), Times.Once);
        _hashServiceMock.Verify(service => service.GetHash(TestPassword), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _userRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<User>(), cancellationToken), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<IdentityAccount>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Exactly(2));
        VerifyNoOtherCalls();
    }

    private AuthService CreateService() =>
        new(
            _userRepositoryMock.Object,
            _identityRepositoryMock.Object,
            _hashServiceMock.Object,
            _jwtServiceMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            NullLogger<AuthService>.Instance);

    private void SetupValidFlow(
        CancellationToken cancellationToken,
        Action<User>? onUserPersist = null,
        Action<IdentityAccount>? onIdentityPersist = null)
    {
        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken))
            .ReturnsAsync((User?)null);

        _hashServiceMock
            .Setup(service => service.GetHash(TestPassword))
            .Returns((ErrorOr<string>)PasswordHash);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _userRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<User>(), cancellationToken))
            .Callback<User, CancellationToken>((user, _) => onUserPersist?.Invoke(user))
            .Returns(Task.CompletedTask);

        _identityRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<IdentityAccount>(), cancellationToken))
            .Callback<IdentityAccount, CancellationToken>((identity, _) => onIdentityPersist?.Invoke(identity))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);
    }

    private void VerifyValidFlow(User user, IdentityAccount identity, CancellationToken cancellationToken)
    {
        _userRepositoryMock.Verify(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken), Times.Once);
        _hashServiceMock.Verify(service => service.GetHash(TestPassword), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _userRepositoryMock.Verify(repository => repository.AddAsync(user, cancellationToken), Times.Once);
        _identityRepositoryMock.Verify(repository => repository.AddAsync(identity, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Exactly(2));
        VerifyNoOtherCalls();
    }

    private void VerifyNoOtherCalls()
    {
        _userRepositoryMock.VerifyNoOtherCalls();
        _identityRepositoryMock.VerifyNoOtherCalls();
        _hashServiceMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }
}
