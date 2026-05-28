namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Desktop.ViewModels;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;
using Shared.Enums;

public class ProfileViewModelTests
{
    private readonly Mock<IProfileService> _profileClientService = new(MockBehavior.Strict);

    [Fact]
    public async Task LoadProfile_WhenApiReturnsProfile_PopulatesProfileDto()
    {
        // Arrange
        const int userId = 1;
        const string email = "test@bank.com";
        const string fullName = "Test User";
        const string phoneNumber = "0712345678";
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        _profileClientService
            .Setup(service => service.GetProfileAsync())
            .ReturnsAsync(new ProfileDto
            {
                UserId = userId,
                Email = email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
            });

        // Act
        bool success = await viewModel.LoadProfile();

        // Assert
        success.Should().BeTrue();
        viewModel.State.Should().Be(ProfileState.UpdateSuccess);
        viewModel.ProfileInfo.FullName.Should().Be(fullName);
        viewModel.ProfileInfo.Email.Should().Be(email);
    }

    [Fact]
    public async Task LoadProfile_WhenApiFails_SetsErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        _profileClientService
            .Setup(service => service.GetProfileAsync())
            .ReturnsAsync(Error.Failure(description: "server down"));

        // Act
        bool success = await viewModel.LoadProfile();

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
    }

    [Fact]
    public void HasPhoneNumber_WhenPhoneNumberIsNotSet_ReturnsFalse()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        // Assert
        viewModel.HasPhoneNumber.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePersonalInfo_WhenUserIdIsNull_SetsErrorState()
    {
        // Arrange
        const string phoneNumber = "0712345678";
        const string address = "123 Main St";
        const string password = "password";
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        // Act
        bool success = await viewModel.UpdatePersonalInfo(phoneNumber, address, password);

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordTooShort_ReturnsError()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string shortPassword = "short";
        var viewModel = new SecurityViewModel(_profileClientService.Object, NullLogger<SecurityViewModel>.Instance);

        // Act
        (bool success, string error) = await viewModel.ChangePassword(
            userId,
            currentPassword,
            shortPassword,
            shortPassword);

        // Assert
        success.Should().BeFalse();
        error.Should().Be(UserMessages.Security.MinimumLengthRequired);
        viewModel.State.Should().Be(ProfileState.Idle);
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordsDoNotMatch_ReturnsError()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string newPassword = "LongEnough1!";
        const string confirmPassword = "Different1!";
        var viewModel = new SecurityViewModel(_profileClientService.Object, NullLogger<SecurityViewModel>.Instance);

        // Act
        (bool success, string error) = await viewModel.ChangePassword(
            userId,
            currentPassword,
            newPassword,
            confirmPassword);

        // Assert
        success.Should().BeFalse();
        error.Should().Be(UserMessages.Security.PasswordMismatch);
        viewModel.State.Should().Be(ProfileState.Idle);
    }

    [Fact]
    public async Task ChangePassword_WhenValid_SetsUpdateSuccessState()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string validPassword = "ValidPass1!";
        var viewModel = new SecurityViewModel(_profileClientService.Object, NullLogger<SecurityViewModel>.Instance);

        _profileClientService
            .Setup(profileClientService => profileClientService.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        (bool success, string error) = await viewModel.ChangePassword(
            userId,
            currentPassword,
            validPassword,
            validPassword);

        // Assert
        success.Should().BeTrue();
        error.Should().BeEmpty();
        viewModel.State.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task ChangePassword_WhenApiReturnsIncorrectPassword_ReturnsSpecificMessage()
    {
        // Arrange
        const int userId = 1;
        const string wrongPassword = "wrong";
        const string validPassword = "ValidPass1!";
        var viewModel = new SecurityViewModel(_profileClientService.Object, NullLogger<SecurityViewModel>.Instance);

        _profileClientService
            .Setup(profileClientService => profileClientService.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(Error.Validation("incorrect_password", "Wrong password"));

        // Act
        (bool success, string error) =
            await viewModel.ChangePassword(userId, wrongPassword, validPassword, validPassword);

        // Assert
        success.Should().BeFalse();
        error.Should().Be(UserMessages.Security.IncorrectPassword);
        viewModel.State.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task ToggleNotificationPreference_WhenApiSucceeds_UpdatesPreference()
    {
        // Arrange
        const int preferenceId = 1;
        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);
        var notificationPreference = new NotificationPreferenceDto
            { Id = preferenceId, EmailEnabled = false };
        viewModel.NotificationPreferences.Add(notificationPreference);

        _profileClientService
            .Setup(profileClientService => profileClientService.UpdateNotificationPreferencesAsync(It.IsAny<List<NotificationPreferenceDto>>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.ToggleNotificationPreference(notificationPreference, true);

        // Assert
        success.Should().BeTrue();
        notificationPreference.EmailEnabled.Should().BeTrue();
        viewModel.State.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task ToggleNotificationPreference_WhenApiFails_RollsBackPreference()
    {
        // Arrange
        const int preferenceId = 1;
        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);
        var notificationPreference = new NotificationPreferenceDto
            { Id = preferenceId, EmailEnabled = true };
        viewModel.NotificationPreferences.Add(notificationPreference);

        _profileClientService
            .Setup(profileClientService => profileClientService.UpdateNotificationPreferencesAsync(It.IsAny<List<NotificationPreferenceDto>>()))
            .ReturnsAsync(Error.Failure(description: "server error"));

        // Act
        bool success = await viewModel.ToggleNotificationPreference(notificationPreference, false);

        // Assert
        success.Should().BeFalse();
        notificationPreference.EmailEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateNotificationPreferences_WhenListIsEmpty_ReturnsFalse()
    {
        // Arrange
        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);

        // Act
        bool result =
            await viewModel.UpdateNotificationPreferences(new List<NotificationPreferenceDto>());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task LoadProfile_WhenPersonalInfoFails_SetsErrorState()
    {
        // Arrange
        _profileClientService
            .Setup(profileClientService => profileClientService.GetProfileAsync())
            .ReturnsAsync(Error.Failure(description: "fail"));

        var profileVm = new ProfileViewModel(
            new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance),
            new SecurityViewModel(_profileClientService.Object, NullLogger<SecurityViewModel>.Instance),
            new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance),
            new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance));

        // Act
        bool success = await profileVm.LoadProfile();

        // Assert
        success.Should().BeFalse();
        profileVm.State.Should().Be(ProfileState.Error);
    }

    [Fact]
    public void IsInitializingView_DefaultsFalse_CanBeToggled()
    {
        // Arrange
        var profileVm = new ProfileViewModel(
            new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance),
            new SecurityViewModel(_profileClientService.Object, NullLogger<SecurityViewModel>.Instance),
            new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance),
            new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance));

        // Assert initial state
        profileVm.IsInitializingView.Should().BeFalse();

        // Act
        profileVm.IsInitializingView = true;

        // Assert toggled state
        profileVm.IsInitializingView.Should().BeTrue();
    }
}
