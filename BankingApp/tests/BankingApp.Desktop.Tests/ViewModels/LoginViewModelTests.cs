namespace BankingApp.Desktop.Tests.ViewModels;

using Contracts.Features.Authentication.Dtos;
using Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Enums;

public class LoginViewModelTests
{
    private readonly Mock<IAuthenticationService> _authenticationServiceMock = new();
    private readonly Mock<IAuthenticationSession> _authenticationSessionMock = new();
    private readonly Mock<ILoginPreferences> _loginPreferencesMock = new();

    public LoginViewModelTests()
    {
        _authenticationSessionMock.Setup(mock => mock.EnsureConfigured()).Returns(Result.Success);
        _authenticationSessionMock.SetupProperty(mock => mock.CurrentUserId);
    }

    [Fact]
    public void CanLogin_WhenValid_ReturnsTrue()
    {
        LoginViewModel.CanLogin("test@test.com", "password").Should().BeTrue();
    }

    [Fact]
    public void CanLogin_WhenInvalid_ReturnsFalse()
    {
        LoginViewModel.CanLogin(string.Empty, "password").Should().BeFalse();
        LoginViewModel.CanLogin("test@test.com", string.Empty).Should().BeFalse();
        LoginViewModel.CanLogin(string.Empty, string.Empty).Should().BeFalse();
        LoginViewModel.CanLogin(" ", " ").Should().BeFalse();
    }

    [Fact]
    public async Task Login_WhenSuccess_ShouldSetLoginStateToSuccessAndSetUserId()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel();
        LoginSuccessResponse response = new() { Token = "test-token", UserId = 1 };

        _authenticationServiceMock
            .Setup(mock => mock.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        _authenticationSessionMock.Setup(mock => mock.SetToken("test-token"));

        // Act
        await viewModel.Login("test@test.com", "password", false);

        // Assert
        viewModel.State.Should().Be(LoginState.Success);
        _authenticationSessionMock.Object.CurrentUserId.Should().Be(1);
        _authenticationServiceMock.VerifyAll();
        _authenticationSessionMock.VerifyAll();
    }

    [Fact]
    public async Task Login_WhenSuccessWithRememberMe_ShouldSavePreferences()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel();
        LoginSuccessResponse response = new() { Token = "test-token", UserId = 1 };

        _authenticationServiceMock
            .Setup(mock => mock.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        _authenticationSessionMock.Setup(mock => mock.SetToken("test-token"));

        // Act
        await viewModel.Login("test@test.com", "password", true);

        // Assert
        viewModel.State.Should().Be(LoginState.Success);
        _loginPreferencesMock.Verify(
            mock => mock.Save("test@test.com", true),
            Times.Once);
    }

    [Fact]
    public async Task Login_WhenSuccessWithoutRememberMe_ShouldSavePreferencesWithFalse()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel();
        LoginSuccessResponse response = new() { Token = "test-token", UserId = 1 };

        _authenticationServiceMock
            .Setup(mock => mock.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        _authenticationSessionMock.Setup(mock => mock.SetToken("test-token"));

        // Act
        await viewModel.Login("test@test.com", "password", false);

        // Assert
        viewModel.State.Should().Be(LoginState.Success);
        _loginPreferencesMock.Verify(
            mock => mock.Save("test@test.com", false),
            Times.Once);
    }

    [Fact]
    public async Task Login_WhenAuthServiceReturnsUnauthorized_ShouldSetLoginStateToInvalidCredentials()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel();

        _authenticationServiceMock
            .Setup(mock => mock.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unauthorized());

        // Act
        await viewModel.Login("test@test.com", "password", false);

        // Assert
        viewModel.State.Should().Be(LoginState.InvalidCredentials);
        _authenticationServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Login_WhenAuthServiceReturnsFailure_ShouldSetLoginStateToError()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel();

        _authenticationServiceMock
            .Setup(mock => mock.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await viewModel.Login("test@test.com", "password", false);

        // Assert
        viewModel.State.Should().Be(LoginState.Error);
        _authenticationServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Login_WhenAuthServiceReturnsForbidden_ShouldSetLoginStateToAccountLocked()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel();

        _authenticationServiceMock
            .Setup(mock => mock.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Forbidden());

        // Act
        await viewModel.Login("test@test.com", "password", false);

        // Assert
        viewModel.State.Should().Be(LoginState.AccountLocked);
        _authenticationServiceMock.VerifyAll();
    }

    [Fact]
    public async Task DevLogin_WhenConfiguredAndSuccessful_ShouldSetLoginStateToSuccessAndSetUserId()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel(new Dictionary<string, string?>
        {
            ["DevLogin:Email"] = "dev@test.com",
            ["DevLogin:Password"] = "password"
        });
        LoginSuccessResponse response = new() { Token = "test-token", UserId = 1 };

        _authenticationServiceMock
            .Setup(mock => mock.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        _authenticationSessionMock.Setup(mock => mock.SetToken("test-token"));

        // Act
        ErrorOr<Success> result = await viewModel.DevLogin();

        // Assert
        result.IsError.Should().BeFalse();
        viewModel.State.Should().Be(LoginState.Success);
        _authenticationSessionMock.Object.CurrentUserId.Should().Be(1);
        _authenticationServiceMock.VerifyAll();
        _authenticationSessionMock.VerifyAll();
    }

    [Fact]
    public async Task DevLogin_WhenMissingConfiguration_ShouldReturnErrorAndNotCallApi()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel();

        // Act
        ErrorOr<Success> result = await viewModel.DevLogin();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("DevLogin.NotConfigured");
        _authenticationServiceMock.Verify(mock => mock.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DevLogin_WhenApiReturnsError_ShouldReturnErrorAndResetState()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel(new Dictionary<string, string?>
        {
            ["DevLogin:Email"] = "dev@test.com",
            ["DevLogin:Password"] = "password"
        });
        var apiError = Error.Unauthorized();

        _authenticationServiceMock
            .Setup(mock => mock.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiError);

        // Act
        ErrorOr<Success> result = await viewModel.DevLogin();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(apiError);
        viewModel.State.Should().Be(LoginState.Idle);
        _authenticationServiceMock.VerifyAll();
    }

    [Fact]
    public async Task DevLogin_WhenTokenMissing_ShouldReturnErrorAndResetState()
    {
        // Arrange
        LoginViewModel viewModel = CreateViewModel(new Dictionary<string, string?>
        {
            ["DevLogin:Email"] = "dev@test.com",
            ["DevLogin:Password"] = "password"
        });
        LoginSuccessResponse response = new() { UserId = 1, Token = null };

        _authenticationServiceMock
            .Setup(mock => mock.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ErrorOr<Success> result = await viewModel.DevLogin();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("DevLogin.MissingToken");
        viewModel.State.Should().Be(LoginState.Idle);
        _authenticationServiceMock.VerifyAll();
    }

    [Fact]
    public void SavedEmail_WhenPreferencesHaveEmail_ShouldReturnEmail()
    {
        // Arrange
        _loginPreferencesMock.Setup(mock => mock.SavedEmail).Returns("saved@test.com");
        _loginPreferencesMock.Setup(mock => mock.RememberMe).Returns(true);
        LoginViewModel viewModel = CreateViewModel();

        // Assert
        viewModel.SavedEmail.Should().Be("saved@test.com");
        viewModel.SavedRememberMe.Should().BeTrue();
    }

    private LoginViewModel CreateViewModel(Dictionary<string, string?>? values = null)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? new Dictionary<string, string?>())
            .Build();

        return new LoginViewModel(
            _authenticationServiceMock.Object,
            _authenticationSessionMock.Object,
            configuration,
            _loginPreferencesMock.Object,
            NullLogger<LoginViewModel>.Instance);
    }
}
