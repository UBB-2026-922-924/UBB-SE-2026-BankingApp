namespace BankingApp.Api.Controllers;

using Application.Features.Authentication.Models;
using Application.Features.Authentication.Services;
using Contracts.Features.Authentication.Dtos;
using Contracts.Features.UserRegistration.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Handles authentication and registration endpoints.
/// </summary>
[ApiController]
[Route(ApiEndpoints.Auth.Base)]
public class AuthController(IAuthService authService) : ApiControllerBase
{
    private const int DeviceInfoMaxLength = 255;
    private const int BrowserMaxLength = 100;
    private const int IpAddressMaxLength = 45;

    [HttpPost(ApiEndpoints.Auth.Login)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(
            await authService.LoginAsync(request.Email, request.Password, GetSessionMetadata(), cancellationToken),
            MapLoginSuccess);
    }

    [HttpPost(ApiEndpoints.Auth.Register)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(
            await authService.RegisterAsync(request.Email, request.Password, request.FullName, cancellationToken));
    }

    [HttpPost(ApiEndpoints.Auth.Logout)]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = AuthHeaderNames.Authorization)] string authorization,
        CancellationToken cancellationToken)
    {
        if (!TryExtractBearerToken(authorization, out string token))
        {
            return Problem(detail: "No token provided.", statusCode: StatusCodes.Status400BadRequest);
        }

        return ToActionResult(await authService.LogoutAsync(token, cancellationToken));
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        string forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        return !string.IsNullOrWhiteSpace(forwardedFor)
            ? forwardedFor.Split(',').First().Trim()
            : context.Connection.RemoteIpAddress?.ToString();
    }

    private static string? GetBrowserName(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return null;
        }

        if (userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase))
        {
            return "Microsoft Edge";
        }

        if (userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase))
        {
            return "Chrome";
        }

        if (userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase))
        {
            return "Firefox";
        }

        if (userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase))
        {
            return "Safari";
        }

        return "Unknown Browser";
    }

    private static string? TrimToMaxLength(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static bool TryExtractBearerToken(string authorization, out string token)
    {
        token = string.Empty;
        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith(AuthHeaderNames.BearerPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        token = authorization[AuthHeaderNames.BearerPrefix.Length..];
        return !string.IsNullOrWhiteSpace(token);
    }

    private IActionResult MapLoginSuccess(LoginSuccess success) =>
        Ok(new LoginSuccessResponse { UserId = success.UserId, Token = success.Token, SessionId = success.SessionId });

    private SessionMetadata GetSessionMetadata()
    {
        string? userAgent = TrimToMaxLength(Request.Headers.UserAgent.ToString(), DeviceInfoMaxLength);
        return new SessionMetadata
        {
            DeviceInfo = userAgent,
            Browser = TrimToMaxLength(GetBrowserName(userAgent), BrowserMaxLength),
            IpAddress = TrimToMaxLength(GetClientIpAddress(HttpContext), IpAddressMaxLength),
        };
    }
}
