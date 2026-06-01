namespace BankingApp.Web.Tests.Controllers;

using System.Globalization;
using System.Security.Claims;
using ClientAuthenticationService = Application.Features.Authentication.Services.IAuthenticationService;
using Contracts.Features.Authentication.Dtos;
using Contracts.Features.UserRegistration.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Web.Controllers;
using BankingApp.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

public sealed class AuthControllerTests : IDisposable
{
    private readonly Mock<ClientAuthenticationService> _authenticationServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IAuthenticationService> _aspNetAuthenticationMock = new(MockBehavior.Strict);
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        DefaultHttpContext httpContext = new();
        ServiceCollection services = new();
        services.AddSingleton(_aspNetAuthenticationMock.Object);
        httpContext.RequestServices = services.BuildServiceProvider();

        _controller = new AuthController(_authenticationServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }

    public void Dispose()
    {
        _controller.Dispose();
    }

    [Fact]
    public void Login_WhenAnonymousGet_ShouldReturnViewWithReturnUrl()
    {
        IActionResult result = _controller.Login("/Transfers");

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        LoginModel model = viewResult.Model.Should().BeOfType<LoginModel>().Subject;
        model.ReturnUrl.Should().Be("/Transfers");
        _authenticationServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Login_WhenAuthenticatedGet_ShouldRedirectToDashboard()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "7")], "Cookies"));

        IActionResult result = _controller.Login();

        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Dashboard");
        _authenticationServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WhenInvalidPost_ShouldReturnViewWithoutCallingApi()
    {
        _controller.ModelState.AddModelError(nameof(LoginModel.Email), "Email is required.");
        LoginModel model = new() { Email = string.Empty, Password = string.Empty };

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _authenticationServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WhenApiReturnsUnauthorized_ShouldShowInvalidCredentialsMessage()
    {
        LoginModel model = new() { Email = "user@example.com", Password = "bad-password" };

        _authenticationServiceMock
            .Setup(service => service.LoginAsync(
                It.Is<LoginRequest>(request => request.Email == model.Email && request.Password == model.Password),
                CancellationToken.None))
            .ReturnsAsync(Error.Unauthorized("auth.invalid", "Invalid email or password."));

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Invalid email or password.");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WhenApiReturnsForbidden_ShouldShowAccountLockedMessage()
    {
        LoginModel model = new() { Email = "user@example.com", Password = "password" };

        _authenticationServiceMock
            .Setup(service => service.LoginAsync(
                It.IsAny<LoginRequest>(),
                CancellationToken.None))
            .ReturnsAsync(Error.Forbidden("auth.locked", "Account locked."));

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Account is locked. Try again later.");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WhenApiReturnsGenericError_ShouldShowGenericMessage()
    {
        LoginModel model = new() { Email = "user@example.com", Password = "password" };

        _authenticationServiceMock
            .Setup(service => service.LoginAsync(
                It.IsAny<LoginRequest>(),
                CancellationToken.None))
            .ReturnsAsync(Error.Failure("auth.failure", "Something broke."));

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Something went wrong. Please try again.");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WhenApiResponseMissingSessionIdPost_ShouldAddErrorAndReturnView()
    {
        LoginModel model = new() { Email = "user@example.com", Password = "ValidPassword1!" };

        _authenticationServiceMock
            .Setup(service => service.LoginAsync(It.IsAny<LoginRequest>(), CancellationToken.None))
            .ReturnsAsync(new LoginSuccessResponse
            {
                UserId = 15,
                Token = "jwt-token",
                SessionId = null
            });

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "The API did not return a session identifier.");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WhenRememberMeTrue_ShouldSetIsPersistentTrue()
    {
        LoginModel model = new()
        {
            Email = "user@example.com",
            Password = "ValidPassword1!",
            RememberMe = true,
            ReturnUrl = "/Dashboard"
        };

        SetupSuccessfulLogin();

        _aspNetAuthenticationMock
            .Setup(service => service.SignInAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<ClaimsPrincipal>(),
                It.Is<AuthenticationProperties>(properties => properties.IsPersistent == true)))
            .Returns(Task.CompletedTask);

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        result.Should().BeOfType<RedirectResult>();
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyAll();
    }

    [Fact]
    public async Task Login_WhenRememberMeFalse_ShouldSetIsPersistentFalse()
    {
        LoginModel model = new()
        {
            Email = "user@example.com",
            Password = "ValidPassword1!",
            RememberMe = false,
            ReturnUrl = "/Dashboard"
        };

        SetupSuccessfulLogin();

        _aspNetAuthenticationMock
            .Setup(service => service.SignInAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<ClaimsPrincipal>(),
                It.Is<AuthenticationProperties>(properties => properties.IsPersistent == false)))
            .Returns(Task.CompletedTask);

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        result.Should().BeOfType<RedirectResult>();
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyAll();
    }

    [Fact]
    public async Task Login_WhenSuccessfulPost_ShouldSignInAndRedirectToReturnUrl()
    {
        LoginModel model = new()
        {
            Email = "user@example.com",
            Password = "ValidPassword1!",
            RememberMe = true,
            ReturnUrl = "/Transfers"
        };
        int userId = 15;
        string token = "jwt-token";
        int sessionId = 3;

        _authenticationServiceMock
            .Setup(service => service.LoginAsync(It.IsAny<LoginRequest>(), CancellationToken.None))
            .ReturnsAsync(new LoginSuccessResponse
            {
                UserId = userId,
                Token = token,
                SessionId = sessionId
            });

        _aspNetAuthenticationMock
            .Setup(service => service.SignInAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.Is<ClaimsPrincipal>(principal =>
                    principal.FindFirstValue(ClaimTypes.NameIdentifier) == userId.ToString("D", CultureInfo.InvariantCulture)
                    && principal.FindFirstValue(AuthClaimTypes.UserId) == userId.ToString("D", CultureInfo.InvariantCulture)
                    && principal.FindFirstValue(AuthClaimTypes.Token) == token
                    && principal.FindFirstValue(AuthClaimTypes.SessionId) == sessionId.ToString(CultureInfo.InvariantCulture)
                    && principal.Identity!.Name == model.Email),
                It.Is<AuthenticationProperties>(properties =>
                    properties.IsPersistent
                    && properties.ExpiresUtc.HasValue)))
            .Returns(Task.CompletedTask);

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Transfers");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyAll();
    }

    [Fact]
    public async Task Logout_WhenSendingPost_ShouldCallApiSignOutCookieAndRedirectToLogin()
    {
        _authenticationServiceMock
            .Setup(service => service.LogoutAsync(CancellationToken.None))
            .ReturnsAsync(Result.Success);

        _aspNetAuthenticationMock
            .Setup(service => service.SignOutAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                null))
            .Returns(Task.CompletedTask);

        IActionResult result = await _controller.Logout(CancellationToken.None);

        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
        
        // Assert that TempData is cleared
        _controller.TempData.Should().BeEmpty();
        
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyAll();
    }

    [Fact]
    public void Register_WhenAnonymousGet_ShouldReturnViewWithEmptyModel()
    {
        IActionResult result = _controller.Register();

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        RegisterModel model = viewResult.Model.Should().BeOfType<RegisterModel>().Subject;
        model.Email.Should().BeEmpty();
        model.FullName.Should().BeEmpty();
        _authenticationServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Register_WhenInvalidPost_ShouldReturnViewWithoutCallingApi()
    {
        _controller.ModelState.AddModelError(nameof(RegisterModel.Email), "Email is required.");
        RegisterModel model = new() { Email = string.Empty, Password = string.Empty, FullName = string.Empty };

        IActionResult result = await _controller.Register(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _authenticationServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Register_WhenApiReturnsConflict_ShouldShowEmailAlreadyExistsMessage()
    {
        RegisterModel model = new() { Email = "user@example.com", Password = "StrongPassword1!", FullName = "John Doe", PasswordConfirmation = "StrongPassword1!" };

        _authenticationServiceMock
            .Setup(service => service.RegisterAsync(
                It.Is<RegisterRequest>(request => request.Email == model.Email && request.Password == model.Password && request.FullName == model.FullName),
                CancellationToken.None))
            .ReturnsAsync(Error.Conflict("auth.conflict", "Conflict."));

        IActionResult result = await _controller.Register(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "This email is already registered.");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Register_WhenApiReturnsInvalidEmail_ShouldShowInvalidEmailMessage()
    {
        RegisterModel model = new() { Email = "bad@example.com", Password = "StrongPassword1!", FullName = "John Doe", PasswordConfirmation = "StrongPassword1!" };

        _authenticationServiceMock
            .Setup(service => service.RegisterAsync(
                It.IsAny<RegisterRequest>(),
                CancellationToken.None))
            .ReturnsAsync(Error.Validation("invalid_email", "Invalid email."));

        IActionResult result = await _controller.Register(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Please enter a valid email address.");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Register_WhenApiReturnsWeakPassword_ShouldShowWeakPasswordMessage()
    {
        RegisterModel model = new() { Email = "user@example.com", Password = "Weakpass1!", FullName = "John Doe", PasswordConfirmation = "Weakpass1!" };

        _authenticationServiceMock
            .Setup(service => service.RegisterAsync(
                It.IsAny<RegisterRequest>(),
                CancellationToken.None))
            .ReturnsAsync(Error.Validation("weak_password", "Weak password."));

        IActionResult result = await _controller.Register(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Password must be at least 8 characters with uppercase, lowercase, a digit and a special character.");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Register_WhenApiReturnsGenericError_ShouldShowGenericMessage()
    {
        RegisterModel model = new() { Email = "user@example.com", Password = "StrongPassword1!", FullName = "John Doe", PasswordConfirmation = "StrongPassword1!" };

        _authenticationServiceMock
            .Setup(service => service.RegisterAsync(
                It.IsAny<RegisterRequest>(),
                CancellationToken.None))
            .ReturnsAsync(Error.Failure("auth.failure", "Failure."));

        IActionResult result = await _controller.Register(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Something went wrong. Please try again.");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Register_WhenSuccessfulPost_ShouldSetTempDataAndRedirectToLogin()
    {
        RegisterModel model = new() { Email = "user@example.com", Password = "StrongPassword1!", FullName = "John Doe", PasswordConfirmation = "StrongPassword1!" };

        _authenticationServiceMock
            .Setup(service => service.RegisterAsync(
                It.Is<RegisterRequest>(request => request.Email == model.Email && request.Password == model.Password && request.FullName == model.FullName),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        IActionResult result = await _controller.Register(model, CancellationToken.None);

        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
        _controller.TempData["Success"].Should().Be("Account created! You can now sign in.");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    private void SetupSuccessfulLogin()
    {
        _authenticationServiceMock
            .Setup(service => service.LoginAsync(It.IsAny<LoginRequest>(), CancellationToken.None))
            .ReturnsAsync(new LoginSuccessResponse
            {
                UserId = 15,
                Token = "jwt-token",
                SessionId = 3
            });
    }
}
