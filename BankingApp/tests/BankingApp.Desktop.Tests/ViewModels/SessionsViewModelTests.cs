namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Desktop.ViewModels;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Enums;

public class SessionsViewModelTests
{
    private readonly Mock<IProfileService> _profileClientService = new(MockBehavior.Strict);

    [Fact]
    public async Task LoadSessionsAsync_WhenApiReturnsSessions_PopulatesCollectionAndResetsState()
    {
        // Arrange
        const int userId = 7;
        var sessions = new List<SessionDto>
        {
            new() { Id = 1, DeviceInfo = "Desktop" },
            new() { Id = 2, DeviceInfo = "Phone" },
        };

        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(service => service.GetSessionsAsync())
            .ReturnsAsync(sessions);

        // Act
        bool success = await viewModel.LoadSessionsAsync(userId);

        // Assert
        success.Should().BeTrue();
        viewModel.State.Should().Be(ProfileState.Idle);
        viewModel.ActiveSessions.Should().BeSameAs(sessions);
    }

    [Fact]
    public async Task LoadSessionsAsync_WhenApiReturnsError_ClearsCollectionAndSetsErrorState()
    {
        // Arrange
        const int userId = 7;
        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(service => service.GetSessionsAsync())
            .ReturnsAsync(Error.Failure(description: "server error"));

        // Act
        bool success = await viewModel.LoadSessionsAsync(userId);

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
        viewModel.ActiveSessions.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadSessionsAsync_WhenApiThrows_ClearsCollectionAndSetsErrorState()
    {
        // Arrange
        const int userId = 7;
        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(profileClientService => profileClientService.GetSessionsAsync())
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        bool success = await viewModel.LoadSessionsAsync(userId);

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
        viewModel.ActiveSessions.Should().BeEmpty();
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenApiSucceeds_ReturnsTrueAndResetsState()
    {
        // Arrange
        const int sessionId = 42;
        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(profileClientService => profileClientService.RevokeSessionAsync(sessionId))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.RevokeSessionAsync(sessionId);

        // Assert
        success.Should().BeTrue();
        viewModel.State.Should().Be(ProfileState.Idle);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenApiReturnsError_SetsErrorState()
    {
        // Arrange
        const int sessionId = 42;
        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(profileClientService => profileClientService.RevokeSessionAsync(sessionId))
            .ReturnsAsync(Error.Failure(description: "revoke failed"));

        // Act
        bool success = await viewModel.RevokeSessionAsync(sessionId);

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenApiThrows_SetsErrorState()
    {
        // Arrange
        const int sessionId = 42;
        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(service => service.RevokeSessionAsync(sessionId))
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        bool success = await viewModel.RevokeSessionAsync(sessionId);

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
    }
}
