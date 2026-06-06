namespace BankingApp.Web.Controllers;

using System.Globalization;
using System.Security.Claims;
using Contracts.Features.Authentication.Dtos;
using Contracts.Features.UserRegistration.Dtos;
using BankingApp.Contracts.Http;
using Models;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientAuthenticationService = Application.Features.Authentication.Services.IAuthenticationService;

public sealed class AuthController : Controller
{
    private readonly ClientAuthenticationService _authenticationService;

    public AuthController(ClientAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(GetSafeReturnUrl(returnUrl) ?? "/Dashboard");
        }

        return View(new LoginModel { ReturnUrl = GetSafeReturnUrl(returnUrl) });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginModel loginModel,
        CancellationToken cancellationToken)
    {
        loginModel.ReturnUrl = GetSafeReturnUrl(loginModel.ReturnUrl);

        if (!ModelState.IsValid)
        {
            return View(loginModel);
        }

        ErrorOr<LoginSuccessResponse> loginResult = await _authenticationService.LoginAsync(
            new LoginRequest
            {
                Email = loginModel.Email,
                Password = loginModel.Password
            },
            cancellationToken);

        if (loginResult.IsError)
        {
            string errorMessage = MapLoginError(loginResult.FirstError);
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(loginModel);
        }

        LoginSuccessResponse loginResponse = loginResult.Value;
        if (string.IsNullOrWhiteSpace(loginResponse.Token))
        {
            ModelState.AddModelError(string.Empty, "The API did not return an authentication token.");
            return View(loginModel);
        }

        if (loginResponse.SessionId is null)
        {
            ModelState.AddModelError(string.Empty, "The API did not return a session identifier.");
            return View(loginModel);
        }

        await SignInUserAsync(
            loginResponse.UserId,
            loginModel.Email,
            loginResponse.Token,
            loginResponse.SessionId.Value,
            loginModel.RememberMe);

        return Redirect(loginModel.ReturnUrl ?? "/Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _authenticationService.LogoutAsync(cancellationToken);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData.Clear();

        return Redirect("/Auth/Login");
    }

    private static string MapLoginError(Error error)
    {
        if (error.Type == ErrorType.Forbidden)
        {
            return "Account is locked. Try again later.";
        }

        if (error.Type == ErrorType.Unauthorized)
        {
            return "Invalid email or password.";
        }

        return "Something went wrong. Please try again.";
    }

    private async Task SignInUserAsync(
        int userId,
        string email,
        string token,
        int sessionId,
        bool rememberMe)
    {
        string userIdValue = userId.ToString("D", CultureInfo.InvariantCulture);
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userIdValue),
            new Claim(ClaimTypes.Name, email),
            new Claim(AuthClaimTypes.UserId, userIdValue),
            new Claim(AuthClaimTypes.Token, token),
            new Claim(AuthClaimTypes.SessionId, sessionId.ToString(CultureInfo.InvariantCulture))
        ];

        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);

        AuthenticationProperties properties = new()
        {
            IsPersistent = rememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterModel registerModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(registerModel);
        }

        ErrorOr<Success> registerResult = await _authenticationService.RegisterAsync(
            new RegisterRequest
            {
                Email = registerModel.Email,
                Password = registerModel.Password,
                FullName = registerModel.FullName
            },
            cancellationToken);

        if (registerResult.IsError)
        {
            string errorMessage = MapRegisterError(registerResult.FirstError);
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(registerModel);
        }

        TempData["Success"] = "Account created! You can now sign in.";
        return Redirect("/Auth/Login");
    }

    private static string MapRegisterError(Error error)
    {
        if (error.Type == ErrorType.Conflict)
        {
            return "This email is already registered.";
        }

        if (error.Code == "invalid_email")
        {
            return "Please enter a valid email address.";
        }

        if (error.Code == "weak_password")
        {
            return "Password must be at least 8 characters with uppercase, lowercase, a digit and a special character.";
        }

        return "Something went wrong. Please try again.";
    }

    private static string? GetSafeReturnUrl(string? returnUrl)
    {
        return IsLocalReturnUrl(returnUrl) ? returnUrl : null;
    }

    private static bool IsLocalReturnUrl(string? returnUrl)
    {
        const int firstLetterOfUrl = 0;
        const int secondLetterOfUrl = 1;
        const int rootPathLength = 1;

        return !string.IsNullOrEmpty(returnUrl)
               && returnUrl[firstLetterOfUrl] == '/'
               && (returnUrl.Length == rootPathLength ||
                   (returnUrl[secondLetterOfUrl] != '/'
                    && returnUrl[secondLetterOfUrl] != '\\'));
    }
}