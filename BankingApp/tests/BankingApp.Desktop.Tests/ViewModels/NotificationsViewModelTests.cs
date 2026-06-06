namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Desktop.ViewModels;
using Domain.Enums;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Enums;

public class NotificationsViewModelTests
{
    private readonly Mock<IProfileService> _profileClientService = new(MockBehavior.Strict);

    [Fact]
    public async Task ToggleNotificationPreference_WhenApiSucceeds_ShouldUpdatePreferenceAndSetSuccessState()
    {
        // Arrange
        var preference = new NotificationPreferenceDto
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = false,
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(preference);

        _profileClientService
            .Setup(service => service.UpdateNotificationPreferencesAsync(viewModel.NotificationPreferences))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.ToggleNotificationPreference(preference, true);

        // Assert
        success.Should().BeTrue();
        preference.EmailEnabled.Should().BeTrue();
        viewModel.State.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task ToggleNotificationPreference_WhenApiFails_ShouldRollbackPreferenceAndSetErrorState()
    {
        // Arrange
        var preference = new NotificationPreferenceDto
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = true,
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(preference);

        _profileClientService
            .Setup(profileClientService => profileClientService.UpdateNotificationPreferencesAsync(viewModel.NotificationPreferences))
            .ReturnsAsync(Error.Failure(description: "save failed"));

        // Act
        bool success = await viewModel.ToggleNotificationPreference(preference, false);

        // Assert
        success.Should().BeFalse();
        preference.EmailEnabled.Should().BeTrue();
        viewModel.State.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task LoadNotificationPreferences_WhenApiReturnsPreferences_ShouldPopulateCollection()
    {
        // Arrange
        var preferences = new List<NotificationPreferenceDto>
        {
            new() { Id = 1, Category = NotificationType.Payment, EmailEnabled = true },
            new() { Id = 2, Category = NotificationType.LowBalance, EmailEnabled = false },
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);

        _profileClientService
            .Setup(service => service.GetNotificationPreferencesAsync())
            .ReturnsAsync(preferences);

        // Act
        bool success = await viewModel.LoadNotificationPreferences();

        // Assert
        success.Should().BeTrue();
        viewModel.NotificationPreferences.Should().BeSameAs(preferences);
        viewModel.State.Should().Be(ProfileState.Idle);
    }

    [Fact]
    public async Task LoadNotificationPreferences_WhenApiFails_ShouldPreserveExistingPreferences()
    {
        // Arrange
        var existingPreference = new NotificationPreferenceDto
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = true,
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(existingPreference);

        _profileClientService
            .Setup(profileClientService => profileClientService.GetNotificationPreferencesAsync())
            .ReturnsAsync(Error.Failure(description: "server down"));

        // Act
        bool success = await viewModel.LoadNotificationPreferences();

        // Assert
        success.Should().BeFalse();
        viewModel.NotificationPreferences.Should().ContainSingle().Which.Should().BeSameAs(existingPreference);
        viewModel.State.Should().Be(ProfileState.Idle);
    }

    [Fact]
    public async Task UpdateNotificationPreferences_WhenApiSucceeds_ShouldReplacePreferencesAndSetSuccessState()
    {
        // Arrange
        var updatedPreferences = new List<NotificationPreferenceDto>
        {
            new() { Id = 1, Category = NotificationType.Payment, EmailEnabled = false },
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);

        _profileClientService
            .Setup(service => service.UpdateNotificationPreferencesAsync(updatedPreferences))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.UpdateNotificationPreferences(updatedPreferences);

        // Assert
        success.Should().BeTrue();
        viewModel.NotificationPreferences.Should().BeSameAs(updatedPreferences);
        viewModel.State.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task UpdateNotificationPreferences_WhenApiFails_ShouldPreserveExistingPreferencesAndSetErrorState()
    {
        // Arrange
        var existingPreference = new NotificationPreferenceDto
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = true,
        };
        var updatedPreferences = new List<NotificationPreferenceDto>
        {
            new() { Id = 2, Category = NotificationType.LowBalance, EmailEnabled = false },
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(existingPreference);

        _profileClientService
            .Setup(profileClientService => profileClientService.UpdateNotificationPreferencesAsync(updatedPreferences))
            .ReturnsAsync(Error.Failure(description: "save failed"));

        // Act
        bool success = await viewModel.UpdateNotificationPreferences(updatedPreferences);

        // Assert
        success.Should().BeFalse();
        viewModel.NotificationPreferences.Should().ContainSingle().Which.Should().BeSameAs(existingPreference);
        viewModel.State.Should().Be(ProfileState.Error);
    }
}
