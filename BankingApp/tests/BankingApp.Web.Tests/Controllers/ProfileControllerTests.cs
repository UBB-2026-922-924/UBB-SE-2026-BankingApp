namespace BankingApp.Web.Tests.Controllers;

using System.Globalization;
using System.Security.Claims;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using BankingApp.Contracts.Http;
using BankingApp.Web.Controllers;
using BankingApp.Web.ViewModels.Profile;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

public sealed class ProfileControllerTests : IDisposable
{
    private const int CurrentSessionId = 5;

    private readonly Mock<IProfileService> _profileServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IAuthenticationService> _aspNetAuthenticationMock = new(MockBehavior.Strict);
    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        DefaultHttpContext httpContext = new();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(AuthClaimTypes.SessionId, CurrentSessionId.ToString(CultureInfo.InvariantCulture))],
            "Cookies"));

        Mock<IUrlHelperFactory> urlHelperFactoryMock = new(MockBehavior.Loose);
        urlHelperFactoryMock
            .Setup(factory => factory.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(Mock.Of<IUrlHelper>());

        ServiceCollection services = new();
        services.AddSingleton(_aspNetAuthenticationMock.Object);
        services.AddSingleton(urlHelperFactoryMock.Object);
        httpContext.RequestServices = services.BuildServiceProvider();

        _controller = new ProfileController(_profileServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public void Index_ShouldRedirectToPersonalInfo()
    {
        // Act
        IActionResult result = _controller.Index();

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.PersonalInfo));
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PersonalInfo_WhenGet_ShouldMapAllFieldsIncludingAddressAndSetIsUnlockedToFalseIfNoTempData()
    {
        // Arrange
        ProfileDto profile = new()
        {
            FullName = "Bob Jones",
            Email = "bob@example.com",
            PhoneNumber = "+9876543210",
            Address = "456 Oak Ave"
        };

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(profile);

        // Act
        IActionResult result = await _controller.PersonalInfo(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        PersonalInfoViewModel model = viewResult.Model.Should().BeOfType<PersonalInfoViewModel>().Subject;
        model.FullName.Should().Be("Bob Jones");
        model.Email.Should().Be("bob@example.com");
        model.PhoneNumber.Should().Be("+9876543210");
        model.Address.Should().Be("456 Oak Ave");
        model.IsUnlocked.Should().BeFalse();
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PersonalInfo_WhenGetWithVerifiedPassword_ShouldSetIsUnlockedToTrue()
    {
        // Arrange
        _controller.TempData["Profile_VerifiedPassword"] = "VerifiedPassword!";
        ProfileDto profile = new()
        {
            FullName = "Bob Jones"
        };

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(profile);

        // Act
        IActionResult result = await _controller.PersonalInfo(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        PersonalInfoViewModel model = viewResult.Model.Should().BeOfType<PersonalInfoViewModel>().Subject;
        model.IsUnlocked.Should().BeTrue();
        _profileServiceMock.VerifyAll();
    }

    [Fact]
    public async Task PersonalInfo_WhenGetFails_ShouldReturnViewWithErrorBanner()
    {
        // Arrange
        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("profile.unavailable", "Service unavailable."));

        // Act
        IActionResult result = await _controller.PersonalInfo(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<PersonalInfoViewModel>();
        ((string?)_controller.TempData["Error"]).Should().NotBeNullOrEmpty();
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PersonalInfo_WhenPostWithoutVerifiedPassword_ShouldRedirectToPersonalInfo()
    {
        // Arrange
        PersonalInfoViewModel model = new();

        // Act
        IActionResult result = await _controller.PersonalInfo(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.PersonalInfo));
        ((string?)_controller.TempData["Error"]).Should().NotBeNullOrEmpty();
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PersonalInfo_WhenPostSucceeds_ShouldUpdateProfileAndClearTempData()
    {
        // Arrange
        _controller.TempData["Profile_VerifiedPassword"] = "VerifiedPassword!";
        PersonalInfoViewModel model = new()
        {
            FullName = "Bob Jones",
            Email = "bob@example.com",
            PhoneNumber = "+9876543210",
            Address = "789 Pine Rd"
        };

        _profileServiceMock
            .Setup(service => service.UpdateProfileAsync(
                It.Is<UpdateProfileRequest>(request =>
                    request.FullName == model.FullName
                    && request.PhoneNumber == model.PhoneNumber
                    && request.Address == model.Address),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.PersonalInfo(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.PersonalInfo));
        _controller.TempData["Success"].Should().Be("Personal information updated successfully.");
        _controller.TempData.Should().NotContainKey("Profile_VerifiedPassword");
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PersonalInfo_WhenPostFails_ShouldReturnViewWithModelErrorAndKeepUnlocked()
    {
        // Arrange
        _controller.TempData["Profile_VerifiedPassword"] = "VerifiedPassword!";
        PersonalInfoViewModel model = new()
        {
            FullName = "Bob Jones",
            Email = "bob@example.com",
        };

        _profileServiceMock
            .Setup(service => service.UpdateProfileAsync(
                It.IsAny<UpdateProfileRequest>(),
                CancellationToken.None))
            .ReturnsAsync(Error.Failure("update.failed", "Update failed."));

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("profile.unavailable", "Unavailable."));

        // Act
        IActionResult result = await _controller.PersonalInfo(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        PersonalInfoViewModel returnedModel = viewResult.Model.Should().BeOfType<PersonalInfoViewModel>().Subject;
        returnedModel.IsUnlocked.Should().BeTrue();
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        _controller.TempData["Profile_VerifiedPassword"].Should().Be("VerifiedPassword!");
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UnlockPersonalInfo_WhenValidPassword_ShouldSetTempDataAndRedirect()
    {
        // Arrange
        PersonalInfoViewModel model = new() { UnlockPassword = "CorrectPassword!" };
        _profileServiceMock
            .Setup(service => service.VerifyPasswordAsync("CorrectPassword!", CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await _controller.UnlockPersonalInfo(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.PersonalInfo));
        _controller.TempData["Profile_VerifiedPassword"].Should().Be("CorrectPassword!");
        _profileServiceMock.VerifyAll();
    }

    [Fact]
    public async Task UnlockPersonalInfo_WhenInvalidPassword_ShouldSetErrorAndRedirect()
    {
        // Arrange
        PersonalInfoViewModel model = new() { UnlockPassword = "WrongPassword!" };
        _profileServiceMock
            .Setup(service => service.VerifyPasswordAsync("WrongPassword!", CancellationToken.None))
            .ReturnsAsync(false);

        // Act
        IActionResult result = await _controller.UnlockPersonalInfo(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.PersonalInfo));
        _controller.TempData.Should().NotContainKey("Profile_VerifiedPassword");
        ((string?)_controller.TempData["Error"]).Should().NotBeNullOrEmpty();
        _profileServiceMock.VerifyAll();
    }

    [Fact]
    public void CancelUpdate_ShouldClearTempDataAndRedirect()
    {
        // Arrange
        _controller.TempData["Profile_VerifiedPassword"] = "VerifiedPassword!";

        // Act
        IActionResult result = _controller.CancelUpdate();

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.PersonalInfo));
        _controller.TempData.Should().NotContainKey("Profile_VerifiedPassword");
    }

    [Fact]
    public async Task Security_WhenGet_ShouldReturnVerifyPasswordView()
    {
        // Arrange
        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(new ProfileDto { FullName = "Test" });

        // Act
        IActionResult result = await _controller.Security(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<VerifyPasswordViewModel>();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Security_WhenPostWithValidPassword_ShouldRedirectToChangePassword()
    {
        // Arrange
        VerifyPasswordViewModel model = new() { CurrentPassword = "CorrectPassword1!" };

        _profileServiceMock
            .Setup(service => service.VerifyPasswordAsync("CorrectPassword1!", CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await _controller.Security(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.ChangePassword));
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Security_WhenPostWithIncorrectPassword_ShouldReturnViewWithError()
    {
        // Arrange
        VerifyPasswordViewModel model = new() { CurrentPassword = "WrongPassword!" };

        _profileServiceMock
            .Setup(service => service.VerifyPasswordAsync("WrongPassword!", CancellationToken.None))
            .ReturnsAsync(false);

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(new ProfileDto { FullName = "Test" });

        // Act
        IActionResult result = await _controller.Security(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Security_WhenPostWithInvalidModel_ShouldReturnView()
    {
        // Arrange
        VerifyPasswordViewModel model = new();
        _controller.ModelState.AddModelError(nameof(VerifyPasswordViewModel.CurrentPassword), "Current password is required.");

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(new ProfileDto { FullName = "Test" });

        // Act
        IActionResult result = await _controller.Security(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangePassword_WhenGetWithoutVerifiedPassword_ShouldRedirectToSecurity()
    {
        // Act
        IActionResult result = await _controller.ChangePassword(CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.Security));
        ((string?)_controller.TempData["Error"]).Should().NotBeNullOrEmpty();
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangePassword_WhenGetWithVerifiedPassword_ShouldReturnChangePasswordView()
    {
        // Arrange
        _controller.TempData["Profile_VerifiedPassword"] = "VerifiedPassword1!";

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(new ProfileDto { FullName = "Test" });

        // Act
        IActionResult result = await _controller.ChangePassword(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<ChangePasswordViewModel>();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangePassword_WhenPostSucceeds_ShouldRedirectToSecurity()
    {
        // Arrange
        _controller.TempData["Profile_VerifiedPassword"] = "OldPassword1!";
        ChangePasswordViewModel model = new()
        {
            NewPassword = "NewPassword1!",
            ConfirmNewPassword = "NewPassword1!"
        };

        _profileServiceMock
            .Setup(service => service.ChangePasswordAsync(
                It.Is<ChangePasswordRequest>(request =>
                    request.CurrentPassword == "OldPassword1!" && request.NewPassword == "NewPassword1!"),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.ChangePassword(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.Security));
        _controller.TempData["Success"].Should().Be("Password changed successfully.");
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangePassword_WhenPostWithoutVerifiedPassword_ShouldRedirectToSecurity()
    {
        // Arrange
        ChangePasswordViewModel model = new()
        {
            NewPassword = "NewPassword1!",
            ConfirmNewPassword = "NewPassword1!"
        };

        // Act
        IActionResult result = await _controller.ChangePassword(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.Security));
        ((string?)_controller.TempData["Error"]).Should().NotBeNullOrEmpty();
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangePassword_WhenServiceReturnsError_ShouldReturnViewWithModelError()
    {
        // Arrange
        _controller.TempData["Profile_VerifiedPassword"] = "OldPassword1!";
        ChangePasswordViewModel model = new()
        {
            NewPassword = "NewPassword1!",
            ConfirmNewPassword = "NewPassword1!"
        };

        _profileServiceMock
            .Setup(service => service.ChangePasswordAsync(
                It.IsAny<ChangePasswordRequest>(),
                CancellationToken.None))
            .ReturnsAsync(Error.Failure("password.change_failed", "Password change failed."));

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(new ProfileDto { FullName = "Test" });

        // Act
        IActionResult result = await _controller.ChangePassword(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangePassword_WhenInvalidModel_ShouldReturnChangePasswordView()
    {
        // Arrange
        _controller.TempData["Profile_VerifiedPassword"] = "OldPassword1!";
        ChangePasswordViewModel model = new();
        _controller.ModelState.AddModelError(nameof(ChangePasswordViewModel.NewPassword), "New password is required.");

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(new ProfileDto { FullName = "Test" });

        // Act
        IActionResult result = await _controller.ChangePassword(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Notifications_WhenGet_ShouldReturnViewWithPreferences()
    {
        // Arrange
        List<NotificationPreferenceDto> preferences =
        [
            new NotificationPreferenceDto
            {
                Category = Domain.Enums.NotificationType.Payment,
                PushEnabled = true,
                EmailEnabled = true,
                SmsEnabled = false,
                MinAmountThreshold = 100m
            }
        ];

        _profileServiceMock
            .Setup(service => service.GetNotificationPreferencesAsync(CancellationToken.None))
            .ReturnsAsync(preferences);

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(new ProfileDto { FullName = "Test" });

        // Act
        IActionResult result = await _controller.Notifications(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        NotificationsViewModel model = viewResult.Model.Should().BeOfType<NotificationsViewModel>().Subject;
        model.Preferences.Should().HaveCount(1);
        model.Preferences[0].PushEnabled.Should().BeTrue();
        model.Preferences[0].EmailEnabled.Should().BeTrue();
        model.Preferences[0].SmsEnabled.Should().BeFalse();
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Notifications_WhenGetFails_ShouldReturnViewWithErrorBanner()
    {
        // Arrange
        _profileServiceMock
            .Setup(service => service.GetNotificationPreferencesAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("notifications.unavailable", "Service unavailable."));

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(new ProfileDto { FullName = "Test" });

        // Act
        IActionResult result = await _controller.Notifications(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        NotificationsViewModel model = viewResult.Model.Should().BeOfType<NotificationsViewModel>().Subject;
        model.Preferences.Should().BeEmpty();
        ((string?)_controller.TempData["Error"]).Should().NotBeNullOrEmpty();
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Notifications_WhenPostSucceeds_ShouldRedirectWithSuccess()
    {
        // Arrange
        NotificationsViewModel viewModel = new()
        {
            Preferences =
            [
                new NotificationPreferenceRowViewModel
                {
                    Category = Domain.Enums.NotificationType.Payment,
                    CategoryDisplayName = "Payment",
                    PushEnabled = true,
                    EmailEnabled = false,
                    SmsEnabled = true,
                    MinAmountThreshold = 50m
                }
            ]
        };

        _profileServiceMock
            .Setup(service => service.UpdateNotificationPreferencesAsync(
                It.Is<List<NotificationPreferenceDto>>(list =>
                    list.Count == 1
                    && list[0].PushEnabled
                    && !list[0].EmailEnabled
                    && list[0].SmsEnabled),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.Notifications(viewModel, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.Notifications));
        _controller.TempData["Success"].Should().Be("Notification preferences saved successfully.");
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Sessions_WhenGet_ShouldCallServiceAndReturnViewWithSessionsAndCurrentSessionIdMarked()
    {
        // Arrange
        DateTime firstSessionCreatedAt = new(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc);
        DateTime secondSessionCreatedAt = new(2026, 4, 20, 14, 30, 0, DateTimeKind.Utc);
        List<SessionDto> sessions =
        [
            new SessionDto { Id = CurrentSessionId, Browser = "Chrome", IpAddress = "127.0.0.1", CreatedAt = firstSessionCreatedAt },
            new SessionDto { Id = 9, Browser = "Firefox", IpAddress = "10.0.0.1", CreatedAt = secondSessionCreatedAt }
        ];

        _profileServiceMock
            .Setup(service => service.GetSessionsAsync(CancellationToken.None))
            .ReturnsAsync(sessions);

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(new ProfileDto { FullName = "Test" });

        // Act
        IActionResult result = await _controller.Sessions(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        SessionsViewModel model = viewResult.Model.Should().BeOfType<SessionsViewModel>().Subject;
        model.Sessions.Should().HaveCount(2);
        model.CurrentSessionId.Should().Be(CurrentSessionId);
        _profileServiceMock.Verify(service => service.GetSessionsAsync(CancellationToken.None), Times.Once);
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Sessions_WhenServiceReturnsError_ShouldSetErrorBannerAndReturnEmptyViewModel()
    {
        // Arrange
        _profileServiceMock
            .Setup(service => service.GetSessionsAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("sessions.unavailable", "Service unavailable."));

        _profileServiceMock
            .Setup(service => service.GetProfileAsync(CancellationToken.None))
            .ReturnsAsync(new ProfileDto { FullName = "Test" });

        // Act
        IActionResult result = await _controller.Sessions(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        SessionsViewModel model = viewResult.Model.Should().BeOfType<SessionsViewModel>().Subject;
        model.Sessions.Should().BeEmpty();
        model.CurrentSessionId.Should().Be(CurrentSessionId);
        ((string?)_controller.TempData["Error"]).Should().NotBeNullOrEmpty();
        _profileServiceMock.Verify(service => service.GetSessionsAsync(CancellationToken.None), Times.Once);
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RevokeSession_WhenServiceReturnsError_ShouldSetErrorBannerAndRedirectToSessions()
    {
        // Arrange
        const int otherSessionId = 9;
        _profileServiceMock
            .Setup(service => service.RevokeSessionAsync(otherSessionId, CancellationToken.None))
            .ReturnsAsync(Error.NotFound("session.not_found", "Session not found."));

        // Act
        IActionResult result = await _controller.RevokeSession(otherSessionId, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.Sessions));
        ((string?)_controller.TempData["Error"]).Should().NotBeNullOrEmpty();
        _profileServiceMock.Verify(service => service.RevokeSessionAsync(otherSessionId, CancellationToken.None), Times.Once);
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RevokeSession_WhenRevokingAnotherSession_ShouldSetSuccessBannerAndRedirectToSessions()
    {
        // Arrange
        const int otherSessionId = 9;
        _profileServiceMock
            .Setup(service => service.RevokeSessionAsync(otherSessionId, CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.RevokeSession(otherSessionId, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.Sessions));
        _controller.TempData["Success"].Should().Be("Session revoked successfully.");
        _profileServiceMock.Verify(service => service.RevokeSessionAsync(otherSessionId, CancellationToken.None), Times.Once);
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Sessions_WhenSessionIdClaimMissing_ShouldSignOutAndRedirectToLogin()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([], "Cookies"));

        _aspNetAuthenticationMock
            .Setup(service => service.SignOutAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                null))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await _controller.Sessions(CancellationToken.None);

        // Assert
        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyAll();
    }

    [Fact]
    public async Task RevokeSession_WhenSessionIdClaimMissing_ShouldSignOutAndRedirectToLogin()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([], "Cookies"));

        _aspNetAuthenticationMock
            .Setup(service => service.SignOutAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                null))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await _controller.RevokeSession(9, CancellationToken.None);

        // Assert
        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyAll();
    }

    [Fact]
    public async Task RevokeSession_WhenRevokingCurrentSession_ShouldSignOutAndRedirectToAuthLogin()
    {
        // Arrange
        _profileServiceMock
            .Setup(service => service.RevokeSessionAsync(CurrentSessionId, CancellationToken.None))
            .ReturnsAsync(Result.Success);

        _aspNetAuthenticationMock
            .Setup(service => service.SignOutAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                null))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await _controller.RevokeSession(CurrentSessionId, CancellationToken.None);

        // Assert
        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyAll();
    }
}
