namespace BankingApp.Application.Tests.Features.UserProfile.Commands;

using Common.Security;
using BankingApp.Application.Features.UserProfile.Services;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.UserProfile.Dtos;
using Domain.Aggregates.UserAggregate.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class UpdateNotificationPreferencesCommandTests
{
    private const int TestUserId = 1;
    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IUserRepository> _userRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new();
    private readonly Mock<IHashService> _hashServiceMock = new();
    private readonly Mock<ISystemClock> _clockMock = new();

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(TestUserId, cancellationToken)).ReturnsAsync((User?)null);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.UpdateNotificationPreferencesAsync(TestUserId, CreatePreferences(), cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
        _userRepositoryMock.Verify(repository => repository.GetByIdAsync(TestUserId, cancellationToken), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSetNotificationPreferences()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        User user = CreateUser();
        SetupValid(user, cancellationToken);
        UserProfileService service = CreateService();

        // Act
        ErrorOr<Success> result = await service.UpdateNotificationPreferencesAsync(TestUserId, CreatePreferences(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        user.NotificationPreferences.Should().ContainSingle();
        NotificationPreference preference = user.NotificationPreferences.Single();
        preference.Category.Should().Be(NotificationType.Payment);
        preference.PushEnabled.Should().BeTrue();
        preference.EmailEnabled.Should().BeFalse();
        preference.SmsEnabled.Should().BeTrue();
        preference.MinAmountThreshold.Should().Be(100m);
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
        ErrorOr<Success> result = await service.UpdateNotificationPreferencesAsync(TestUserId, CreatePreferences(), cancellationToken);

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

    private static List<NotificationPreferenceDto> CreatePreferences() =>
        [
            new NotificationPreferenceDto
            {
                Category = NotificationType.Payment,
                PushEnabled = true,
                EmailEnabled = false,
                SmsEnabled = true,
                MinAmountThreshold = 100m
            }
        ];

    private void SetupValid(User user, CancellationToken cancellationToken)
    {
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(TestUserId, cancellationToken)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repository => repository.UpdateAsync(user, cancellationToken)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);
    }

    private void VerifyValid(User user, CancellationToken cancellationToken)
    {
        _userRepositoryMock.Verify(repository => repository.GetByIdAsync(TestUserId, cancellationToken), Times.Once);
        _userRepositoryMock.Verify(repository => repository.UpdateAsync(user, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    private static User CreateUser() => User.Register(Email.Create("test@example.com").Value, "Test User", _testNow);
}
