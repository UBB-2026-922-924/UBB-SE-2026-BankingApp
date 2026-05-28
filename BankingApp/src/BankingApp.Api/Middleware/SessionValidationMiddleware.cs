namespace BankingApp.Api.Middleware;

using System.Globalization;
using System.Security.Claims;
using Application.Common.Security;
using Contracts.Http;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.IdentityAggregate.Entities;
using Domain.Repositories;
using ErrorOr;
using Logging;

/// <summary>
///     Validates bearer tokens and active sessions on non-public endpoints.
/// </summary>
public class SessionValidationMiddleware
{
    private static readonly string[] _publicEndpointPrefixes = [$"/{ApiEndpoints.Auth.Base}/", "/swagger"];
    private readonly RequestDelegate _next;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionValidationMiddleware" /> class.
    /// </summary>
    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    ///     Validates the authorization token and backing session, then invokes the next middleware.
    /// </summary>
    public async Task Invoke(
        HttpContext context,
        IIdentityRepository identityRepository,
        IJsonWebTokenService jsonWebTokenService,
        ILogger<SessionValidationMiddleware> logger)
    {
        string? path = context.Request.Path.Value?.ToLower(CultureInfo.InvariantCulture);
        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

        if (!TryExtractBearerToken(context, out string token))
        {
            await RejectRequest(context, "No token provided.");
            return;
        }

        ErrorOr<int> userIdResult = jsonWebTokenService.ExtractUserId(token);
        if (userIdResult.IsError)
        {
            logger.TokenValidationFailed(userIdResult.FirstError.Code, userIdResult.FirstError.Description);
            await RejectRequest(context, "Invalid or expired token.");
            return;
        }

        IdentityAccount? identity = await identityRepository.GetBySessionTokenAsync(token, context.RequestAborted);
        Session? session = identity?.Sessions.FirstOrDefault(candidate => candidate.Token == token);
        if (identity is null || session is null || session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow ||
            identity.UserId != userIdResult.Value)
        {
            logger.SessionLookupFailed("session_not_found", "Session was not found or is no longer active.");
            await RejectRequest(context, "Invalid or expired token.");
            return;
        }

        context.Items["UserId"] = userIdResult.Value;
        ClaimsIdentity claimsIdentity = new(
            [new Claim(AuthClaimTypes.UserId, userIdResult.Value.ToString(CultureInfo.InvariantCulture))],
            authenticationType: "Session");
        context.User = new ClaimsPrincipal(claimsIdentity);
        await _next(context);
    }

    private static bool IsPublicEndpoint(string? path)
    {
        return path is not null &&
               Array.Exists(_publicEndpointPrefixes, prefix => path.StartsWith(prefix, StringComparison.Ordinal));
    }

    private static bool TryExtractBearerToken(HttpContext context, out string token)
    {
        token = string.Empty;
        string? authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith(AuthHeaderNames.BearerPrefix, StringComparison.Ordinal) != true)
        {
            return false;
        }

        token = authHeader[AuthHeaderNames.BearerPrefix.Length..];
        return !string.IsNullOrWhiteSpace(token);
    }

    private static async Task RejectRequest(HttpContext context, string error)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error });
    }
}
