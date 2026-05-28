namespace BankingApp.Application.Tests.Features.UserProfile.Queries;

using BankingApp.Application.Common.Security;
using BankingApp.Application.Features.UserProfile.Services;
using BankingApp.Contracts.Features.UserProfile.Dtos;
using BankingApp.Domain.Aggregates.IdentityAggregate.Entities;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class GetActiveSessionsQueryTests
{
    private const int TestUserId = 42;

    private static readonly DateTime _testNow = new(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IIdentityRepository> _identityRepositoryMock = MockFactory.CreateIdentityRepositoryMock();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IHashService> _hashServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISystemClock> _clockMock = new();

    [Fact]
    public async Task Handle_WhenIdentityNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        UserProfileService service = CreateService();

        // Act
        ErrorOr<List<SessionDto>> result = await service.GetActiveSessionsAsync(TestUserId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _identityRepositoryMock.Verify(
            repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None),
            Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenIdentityHasMixedSessions_ShouldReturnOnlyActiveSessions()
    {
        // Arrange
        (_, IdentityAccount identity) = MockFactory.CreateUserWithIdentityAccount();
        identity.OpenSession("active-token", _testNow.AddHours(24), _testNow, "Windows", "Chrome", "1.2.3.4");
        Session revokedSession = identity.OpenSession("revoked-token", _testNow.AddHours(24), _testNow, "Mac", "Safari", "5.6.7.8");
        revokedSession.Revoke();

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None))
            .ReturnsAsync(identity);

        UserProfileService service = CreateService();

        // Act
        ErrorOr<List<SessionDto>> result = await service.GetActiveSessionsAsync(TestUserId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle(sessionDto => sessionDto.Browser == "Chrome");
        _identityRepositoryMock.Verify(
            repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None),
            Times.Once);
        _identityRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSessionExists_ShouldMapAllFieldsIncludingCreatedAt()
    {
        // Arrange
        DateTime sessionCreatedAt = new(2026, 4, 15, 9, 0, 0, DateTimeKind.Utc);
        const string deviceInfo = "Desktop";
        const string browser = "Firefox";
        const string ipAddress = "192.168.1.1";

        (_, IdentityAccount identity) = MockFactory.CreateUserWithIdentityAccount();
        identity.OpenSession("token-abc", _testNow.AddHours(24), sessionCreatedAt, deviceInfo, browser, ipAddress);

        _identityRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(TestUserId, CancellationToken.None))
            .ReturnsAsync(identity);

        UserProfileService service = CreateService();

        // Act
        ErrorOr<List<SessionDto>> result = await service.GetActiveSessionsAsync(TestUserId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        SessionDto sessionDto = result.Value.Should().ContainSingle().Subject;
        sessionDto.DeviceInfo.Should().Be(deviceInfo);
        sessionDto.Browser.Should().Be(browser);
        sessionDto.IpAddress.Should().Be(ipAddress);
        sessionDto.CreatedAt.Should().Be(sessionCreatedAt);
        sessionDto.LastActiveAt.Should().BeNull();
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
