namespace BankingApp.Application.Tests.Features.Authentication.Services;

using Application.Features.Authentication.Services;
using BankingApp.Domain.Common.Errors;
using Common.Security;
using Contracts.Features.Authentication.Dtos;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class AuthServiceTests
{
    private const string TestEmail = "user@test.com";
    private const string TestPassword = "Password123!";
    private const string PasswordHash = "password-hash";
    private const string SessionToken = "session-token";

    private static readonly DateTime _testNow = new(2026, 6, 5, 8, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IUserRepository> _userRepositoryMock = MockFactory.CreateUserRepositoryMock();
    private readonly Mock<IIdentityRepository> _identityRepositoryMock = MockFactory.CreateIdentityRepositoryMock();
    private readonly Mock<IHashService> _hashServiceMock = MockFactory.CreateHashServiceMock();
    private readonly Mock<IJsonWebTokenService> _jwtServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISystemClock> _clockMock = new();

    [Fact]
    public async Task LoginAsync_WhenEmailIsInvalid_ShouldReturnInvalidCredentials()
    {
        AuthService service = CreateService();

        ErrorOr<LoginSuccess> result = await service.LoginAsync("not-an-email", TestPassword, null, TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthErrors.InvalidCredentials);
        VerifyNoRepositoryWrites();
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldReturnInvalidCredentials()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        AuthService service = CreateService();

        ErrorOr<LoginSuccess> result = await service.LoginAsync(TestEmail, TestPassword, null, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthErrors.InvalidCredentials);
        _userRepositoryMock.Verify(repository => repository.GetByEmailAsync(It.IsAny<Email>(), cancellationToken), Times.Once);
        VerifyNoRepositoryWrites();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ShouldReturnInvalidCredentials()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        (User user, IdentityAccount identity) = MockFactory.CreateUserWithIdentityAccount(TestEmail, PasswordHash, _testNow);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken))
            .ReturnsAsync(user);

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(identity);

        _hashServiceMock
            .Setup(service => service.Verify(TestPassword, PasswordHash))
            .Returns(false);

        _identityRepositoryMock
            .Setup(repository => repository.UpdateAsync(identity, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        AuthService service = CreateService();

        ErrorOr<LoginSuccess> result = await service.LoginAsync(TestEmail, TestPassword, null, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthErrors.InvalidCredentials);
        identity.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public async Task LoginAsync_WhenAccountIsLocked_ShouldReturnAccountLocked()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        (User user, IdentityAccount identity) = MockFactory.CreateUserWithIdentityAccount(TestEmail, PasswordHash, _testNow);
        identity.LockAccount(_testNow.AddMinutes(15));

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken))
            .ReturnsAsync(user);

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(identity);

        AuthService service = CreateService();

        ErrorOr<LoginSuccess> result = await service.LoginAsync(TestEmail, TestPassword, null, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthErrors.AccountLocked);
        _hashServiceMock.Verify(service => service.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnTokenAndSession()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        (User user, IdentityAccount identity) = MockFactory.CreateUserWithIdentityAccount(TestEmail, PasswordHash, _testNow);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<Email>(email => email.Value == TestEmail), cancellationToken))
            .ReturnsAsync(user);

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(identity);

        _hashServiceMock
            .Setup(service => service.Verify(TestPassword, PasswordHash))
            .Returns(true);

        _jwtServiceMock
            .Setup(service => service.GenerateToken(user.Id))
            .Returns((ErrorOr<string>)"jwt-token");

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _identityRepositoryMock
            .Setup(repository => repository.UpdateAsync(identity, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        AuthService service = CreateService();

        ErrorOr<LoginSuccess> result = await service.LoginAsync(TestEmail, TestPassword, null, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Token.Should().Be("jwt-token");
        result.Value.UserId.Should().Be(user.Id);
        identity.Sessions.Should().ContainSingle(session => session.Token == "jwt-token");
    }

    [Fact]
    public async Task LogoutAsync_WhenSessionIsNotFound_ShouldReturnSessionNotFound()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        AuthService service = CreateService();

        ErrorOr<Success> result = await service.LogoutAsync(SessionToken, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthErrors.SessionNotFound);
        _identityRepositoryMock.Verify(repository => repository.GetBySessionTokenAsync(SessionToken, cancellationToken), Times.Once);
        VerifyNoRepositoryWrites();
    }

    [Fact]
    public async Task LogoutAsync_WhenSessionExists_ShouldRevokeSessionAndSaveChanges()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        (_, IdentityAccount identity) = MockFactory.CreateUserWithIdentityAccount();
        identity.OpenSession(SessionToken, _testNow.AddHours(1), _testNow);

        _identityRepositoryMock
            .Setup(repository => repository.GetBySessionTokenAsync(SessionToken, cancellationToken))
            .ReturnsAsync(identity);

        _identityRepositoryMock
            .Setup(repository => repository.UpdateAsync(identity, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        AuthService service = CreateService();

        ErrorOr<Success> result = await service.LogoutAsync(SessionToken, cancellationToken);

        result.IsError.Should().BeFalse();
        identity.Sessions.Should().OnlyContain(session => session.IsRevoked);
        _identityRepositoryMock.Verify(repository => repository.UpdateAsync(identity, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
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

    private void VerifyNoRepositoryWrites()
    {
        _identityRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<IdentityAccount>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
