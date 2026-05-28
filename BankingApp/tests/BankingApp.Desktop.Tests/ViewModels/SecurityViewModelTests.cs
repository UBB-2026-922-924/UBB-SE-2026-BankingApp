namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Desktop.ViewModels;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;
using Shared.Enums;

public class SecurityViewModelTests
{
    private readonly Mock<IProfileService> _profileClientService;
    private readonly SecurityViewModel _viewModel;

    public SecurityViewModelTests()
    {
        _profileClientService = new Mock<IProfileService>();
        _viewModel = new SecurityViewModel(_profileClientService.Object, NullLogger<SecurityViewModel>.Instance);
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordTooShort_ReturnsFalseWithLengthError()
    {
        // Act
        (bool Success, string ErrorMessage) result = await _viewModel.ChangePassword(1, "OldPass123!", "Short1!", "Short1!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.MinimumLengthRequired, result.ErrorMessage);
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordsDoNotMatch_ReturnsFalseWithMismatchError()
    {
        // Act
        (bool Success, string ErrorMessage) result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "Different123!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.PasswordMismatch, result.ErrorMessage);
    }

    [Fact]
    public async Task ChangePassword_WhenDataIsValid_ReturnsSuccessAndUpdatesState()
    {
        // Arrange
        _profileClientService
            .Setup(profileClientService => profileClientService.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        (bool Success, string ErrorMessage) result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "NewPass123!");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ProfileState.UpdateSuccess, _viewModel.State);
    }

    [Fact]
    public async Task ChangePassword_WhenApiReturnsIncorrectPasswordError_UpdatesStateAndReturnsSpecificMessage()
    {
        // Arrange
        var error = Error.Validation("incorrect_password", "Description");
        _profileClientService
            .Setup(profileClientService => profileClientService.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(error);

        // Act
        (bool Success, string ErrorMessage) result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "NewPass123!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.IncorrectPassword, result.ErrorMessage);
        Assert.Equal(ProfileState.Error, _viewModel.State);
    }

    [Fact]
    public async Task ChangePassword_WhenApiReturnsGenericError_UpdatesStateAndReturnsGenericMessage()
    {
        // Arrange
        var error = Error.Failure("server_error", "Description");
        _profileClientService
            .Setup(profileClientService => profileClientService.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(error);

        // Act
        (bool Success, string ErrorMessage) result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "NewPass123!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.UnexpectedError, result.ErrorMessage);
        Assert.Equal(ProfileState.Error, _viewModel.State);
    }

}
