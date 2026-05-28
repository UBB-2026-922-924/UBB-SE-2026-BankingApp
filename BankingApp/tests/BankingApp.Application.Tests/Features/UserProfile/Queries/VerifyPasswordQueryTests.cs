namespace BankingApp.Application.Tests.Features.UserProfile.Queries;

using BankingApp.Application.Common.Security;
using BankingApp.Application.Features.UserProfile.Services;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class VerifyPasswordQueryTests
{
    private const int TestUserId = 1;
    private const string Password = "Password123!";
    private const string PasswordHash = "hash";

    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IHashService> _hashServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISystemClock> _clockMock = new();

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync((IdentityAccount?)null);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<bool> result = await service.VerifyPasswordAsync(TestUserId, Password, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _hashServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ShouldReturnNotFoundError()
    {
        await Handle_WhenUserNotFound_ShouldReturnNotFoundError();
    }

    [Fact]
    public async Task Handle_WhenPasswordDoesNotMatch_ShouldReturnFalse()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap(PasswordHash));
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync(identity);
        _hashServiceMock.Setup(service => service.Verify(Password, PasswordHash)).Returns((ErrorOr<bool>)false);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<bool> result = await service.VerifyPasswordAsync(TestUserId, Password, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeFalse();
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _hashServiceMock.Verify(service => service.Verify(Password, PasswordHash), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _hashServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenPasswordMatches_ShouldReturnTrue()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var identity = IdentityAccount.Create(TestUserId, HashedPassword.Wrap(PasswordHash));
        _identityRepositoryMock.Setup(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken)).ReturnsAsync(identity);
        _hashServiceMock.Setup(service => service.Verify(Password, PasswordHash)).Returns((ErrorOr<bool>)true);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<bool> result = await service.VerifyPasswordAsync(TestUserId, Password, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
        _identityRepositoryMock.Verify(repository => repository.GetByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _hashServiceMock.Verify(service => service.Verify(Password, PasswordHash), Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
        _hashServiceMock.VerifyNoOtherCalls();
    }

    private UserProfileService CreateService() =>
        new(
            _userRepositoryMock.Object,
            _identityRepositoryMock.Object,
            _hashServiceMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            NullLogger<UserProfileService>.Instance);
}
