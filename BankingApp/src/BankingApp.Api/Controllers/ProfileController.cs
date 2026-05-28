namespace BankingApp.Api.Controllers;

using Application.Features.UserProfile.Services;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Exposes profile, password, notification, and session management endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route(ApiEndpoints.Profile.Base)]
public class ProfileController(IUserProfileService userProfileService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await userProfileService.GetProfileAsync(userId, cancellationToken), Ok);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await userProfileService.UpdateProfileAsync(
                userId, request.FullName, request.PhoneNumber, request.DateOfBirth,
                request.Address, request.Nationality, request.PreferredLanguage, cancellationToken));
    }

    [HttpPut(ApiEndpoints.Profile.ChangePassword)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await userProfileService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken));
    }

    [HttpGet(ApiEndpoints.Profile.NotificationPreferences)]
    public async Task<IActionResult> GetNotificationPreferences(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await userProfileService.GetNotificationPreferencesAsync(userId, cancellationToken), Ok);
    }

    [HttpPut(ApiEndpoints.Profile.NotificationPreferences)]
    public async Task<IActionResult> UpdateNotificationPreferences(
        [FromBody] List<NotificationPreferenceDto> preferences,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await userProfileService.UpdateNotificationPreferencesAsync(userId, preferences, cancellationToken));
    }

    [HttpPost(ApiEndpoints.Profile.VerifyPassword)]
    public async Task<IActionResult> VerifyPassword([FromBody] string password, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await userProfileService.VerifyPasswordAsync(userId, password, cancellationToken),
            isValid => Ok(isValid));
    }

    [HttpGet(ApiEndpoints.Profile.Sessions)]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await userProfileService.GetActiveSessionsAsync(userId, cancellationToken), Ok);
    }

    [HttpDelete(ApiEndpoints.Profile.SessionById)]
    public async Task<IActionResult> RevokeSession(int sessionId, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await userProfileService.RevokeSessionAsync(userId, sessionId, cancellationToken));
    }
}
