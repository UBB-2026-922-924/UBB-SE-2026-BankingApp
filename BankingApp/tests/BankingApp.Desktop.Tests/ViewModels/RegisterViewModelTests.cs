namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Desktop.ViewModels;
using Contracts.Features.UserRegistration.Dtos;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Enums;

public class RegisterViewModelTests
{
    private readonly Mock<IAuthenticationService> _authenticationService = new();

    [Fact]
    public async Task Register_WhenEmptyFields_ShouldSetErrorState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _authenticationService.Object,
            NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register(string.Empty, "pass", "pass", "Name");

        // Assert
        viewModel.State.Should().Be(RegisterState.Error);
    }

    [Fact]
    public async Task Register_WhenPasswordMismatch_ShouldSetPasswordMismatchState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _authenticationService.Object,
            NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register("test@test.com", "Password123!", "Password123", "Name");

        // Assert
        viewModel.State.Should().Be(RegisterState.PasswordMismatch);
    }

    [Fact]
    public async Task Register_WhenWeakPassword_ShouldSetWeakPasswordState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _authenticationService.Object,
            NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register("test@test.com", "weak", "weak", "Name");

        // Assert
        viewModel.State.Should().Be(RegisterState.WeakPassword);
    }

    [Fact]
    public async Task Register_WhenValid_ShouldSetSuccessState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _authenticationService.Object,
            NullLogger<RegisterViewModel>.Instance);

        _authenticationService
            .Setup(authenticationService => authenticationService.RegisterAsync(
                It.IsAny<RegisterRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        // Act
        await viewModel.Register("test@test.com", "StrongP@ss1", "StrongP@ss1", "Name");

        // Assert
        viewModel.State.Should().Be(RegisterState.Success);
    }

    [Fact]
    public async Task Register_WhenEmailConflicts_ShouldSetEmailAlreadyExistsState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(
            _authenticationService.Object,
            NullLogger<RegisterViewModel>.Instance);

        _authenticationService
            .Setup(authenticationService => authenticationService.RegisterAsync(
                It.IsAny<RegisterRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict("Conflict", "Conflict"));

        // Act
        await viewModel.Register("test@test.com", "StrongP@ss1", "StrongP@ss1", "Name");

        // Assert
        viewModel.State.Should().Be(RegisterState.EmailAlreadyExists);
    }
}
