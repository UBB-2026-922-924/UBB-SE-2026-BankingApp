namespace BankingApp.Web.Controllers;

using BankingApp.Contracts.Http;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using Contracts.Features.UserProfile.Validation;
using Domain.Common.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Profile;

[Authorize]
public class ProfileController(IProfileService profileService) : Controller
{
    private const string VerifiedPasswordSessionKey = "Profile_VerifiedPassword";

    public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        string actionName = context.ActionDescriptor.RouteValues["action"] ?? string.Empty;
        if (actionName != nameof(ChangePassword) && 
            actionName != nameof(Security) &&
            actionName != nameof(PersonalInfo) &&
            actionName != nameof(UnlockPersonalInfo) &&
            actionName != nameof(CancelUpdate))
        {
            TempData.Remove(VerifiedPasswordSessionKey);
        }
    }

    public IActionResult Index() => RedirectToAction(nameof(PersonalInfo));

    public async Task<IActionResult> PersonalInfo(CancellationToken cancellationToken)
    {
        await PopulateSidebarAsync(cancellationToken);

        ErrorOr<ProfileDto> result = await profileService.GetProfileAsync(cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load personal information.";
            return View(new PersonalInfoViewModel());
        }

        bool isUnlocked = TempData.Peek(VerifiedPasswordSessionKey) is string;

        ProfileDto profile = result.Value;
        PersonalInfoViewModel viewModel = new()
        {
            FullName = profile.FullName ?? string.Empty,
            Email = profile.Email ?? string.Empty,
            PhoneNumber = profile.PhoneNumber,
            Address = profile.Address,
            IsUnlocked = isUnlocked
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersonalInfo(PersonalInfoViewModel model, CancellationToken cancellationToken)
    {
        if (TempData[VerifiedPasswordSessionKey] is not string verifiedPassword)
        {
            TempData["Error"] = "Please unlock your profile first.";
            return RedirectToAction(nameof(PersonalInfo));
        }

        if (!ModelState.IsValid)
        {
            TempData[VerifiedPasswordSessionKey] = verifiedPassword;
            model.IsUnlocked = true;
            await PopulateSidebarAsync(cancellationToken);
            return View(model);
        }

        UpdateProfileRequest request = new()
        {
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Address = model.Address,
        };

        ErrorOr<Success> result = await profileService.UpdateProfileAsync(request, cancellationToken);

        if (result.IsError)
        {
            ModelState.AddModelError(string.Empty, "Could not update personal information. Please try again.");
            TempData[VerifiedPasswordSessionKey] = verifiedPassword;
            model.IsUnlocked = true;
            await PopulateSidebarAsync(cancellationToken);
            return View(model);
        }

        TempData.Remove(VerifiedPasswordSessionKey);
        TempData["Success"] = "Personal information updated successfully.";
        return RedirectToAction(nameof(PersonalInfo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockPersonalInfo(PersonalInfoViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(model.UnlockPassword))
        {
            TempData["Error"] = "Please enter your password to unlock.";
            return RedirectToAction(nameof(PersonalInfo));
        }

        ErrorOr<bool> result = await profileService.VerifyPasswordAsync(model.UnlockPassword, cancellationToken);
        if (result.IsError || !result.Value)
        {
            TempData["Error"] = "Incorrect password. Please try again.";
            return RedirectToAction(nameof(PersonalInfo));
        }

        TempData[VerifiedPasswordSessionKey] = model.UnlockPassword;
        TempData["Success"] = "Profile unlocked for editing.";
        return RedirectToAction(nameof(PersonalInfo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CancelUpdate()
    {
        TempData.Remove(VerifiedPasswordSessionKey);
        return RedirectToAction(nameof(PersonalInfo));
    }

    public async Task<IActionResult> Notifications(CancellationToken cancellationToken)
    {
        await PopulateSidebarAsync(cancellationToken);

        ErrorOr<List<NotificationPreferenceDto>> result =
            await profileService.GetNotificationPreferencesAsync(cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load notification preferences. Please try again.";
            return View(new NotificationsViewModel());
        }

        NotificationsViewModel viewModel = new()
        {
            Preferences = result.Value.ConvertAll(preference => new NotificationPreferenceRowViewModel
            {
                Category = preference.Category,
                CategoryDisplayName = preference.Category.ToDisplayName(),
                PushEnabled = preference.PushEnabled,
                EmailEnabled = preference.EmailEnabled,
                SmsEnabled = preference.SmsEnabled,
                MinAmountThreshold = preference.MinAmountThreshold
            })
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Notifications(NotificationsViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSidebarAsync(cancellationToken);
            return View(viewModel);
        }

        var preferencesToSave = viewModel.Preferences
            .Select(row => new NotificationPreferenceDto
            {
                Category = row.Category,
                PushEnabled = row.PushEnabled,
                EmailEnabled = row.EmailEnabled,
                SmsEnabled = row.SmsEnabled,
                MinAmountThreshold = row.MinAmountThreshold
            })
            .ToList();

        ErrorOr<Success> saveResult =
            await profileService.UpdateNotificationPreferencesAsync(preferencesToSave, cancellationToken);

        TempData[saveResult.IsError ? "Error" : "Success"] = saveResult.IsError
            ? "Could not save notification preferences. Please try again."
            : "Notification preferences saved successfully.";

        return RedirectToAction(nameof(Notifications));
    }

    [HttpGet]
    public async Task<IActionResult> Security(CancellationToken cancellationToken)
    {
        await PopulateSidebarAsync(cancellationToken);
        return View(new VerifyPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Security(VerifyPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSidebarAsync(cancellationToken);
            return View(model);
        }

        ErrorOr<bool> result = await profileService.VerifyPasswordAsync(model.CurrentPassword, cancellationToken);

        if (result.IsError || !result.Value)
        {
            ModelState.AddModelError(string.Empty, "Incorrect password. Please try again.");
            await PopulateSidebarAsync(cancellationToken);
            return View(model);
        }

        TempData[VerifiedPasswordSessionKey] = model.CurrentPassword;
        return RedirectToAction(nameof(ChangePassword));
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword(CancellationToken cancellationToken)
    {
        if (TempData.Peek(VerifiedPasswordSessionKey) is not string)
        {
            TempData["Error"] = "Please verify your current password first.";
            return RedirectToAction(nameof(Security));
        }

        await PopulateSidebarAsync(cancellationToken);
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken)
    {
        if (TempData[VerifiedPasswordSessionKey] is not string verifiedPassword)
        {
            TempData["Error"] = "Please verify your current password first.";
            return RedirectToAction(nameof(Security));
        }

        if (!ModelState.IsValid)
        {
            TempData[VerifiedPasswordSessionKey] = verifiedPassword;
            await PopulateSidebarAsync(cancellationToken);
            return View(model);
        }

        if (!PasswordValidator.MeetsMinimumLength(model.NewPassword))
        {
            ModelState.AddModelError(nameof(model.NewPassword), "Password must be at least 8 characters.");
            TempData[VerifiedPasswordSessionKey] = verifiedPassword;
            await PopulateSidebarAsync(cancellationToken);
            return View(model);
        }

        ChangePasswordRequest request = new()
        {
            CurrentPassword = verifiedPassword,
            NewPassword = model.NewPassword,
        };

        ErrorOr<Success> result = await profileService.ChangePasswordAsync(request, cancellationToken);

        if (result.IsError)
        {
            ModelState.AddModelError(string.Empty, result.FirstError.Description);
            TempData[VerifiedPasswordSessionKey] = verifiedPassword;
            await PopulateSidebarAsync(cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction(nameof(Security));
    }

    public async Task<IActionResult> Sessions(CancellationToken cancellationToken)
    {
        int? currentSessionId = ParseCurrentSessionId();
        if (currentSessionId is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Auth/Login");
        }

        await PopulateSidebarAsync(cancellationToken);

        ErrorOr<List<SessionDto>> result = await profileService.GetSessionsAsync(cancellationToken);
        if (result.IsError)
        {
            TempData["Error"] = "Could not load sessions.";
            return View(new SessionsViewModel { Sessions = [], CurrentSessionId = currentSessionId.Value });
        }

        return View(new SessionsViewModel { Sessions = result.Value, CurrentSessionId = currentSessionId.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Profile/RevokeSession/{sessionId:int}")]
    public async Task<IActionResult> RevokeSession(int sessionId, CancellationToken cancellationToken)
    {
        int? currentSessionId = ParseCurrentSessionId();
        if (currentSessionId is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Auth/Login");
        }

        ErrorOr<Success> result = await profileService.RevokeSessionAsync(sessionId, cancellationToken);
        if (result.IsError)
        {
            TempData["Error"] = "Could not revoke that session.";
            return RedirectToAction(nameof(Sessions));
        }

        if (sessionId == currentSessionId.Value)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Auth/Login");
        }

        TempData["Success"] = "Session revoked successfully.";
        return RedirectToAction(nameof(Sessions));
    }

    private int? ParseCurrentSessionId()
    {
        string? claim = User.FindFirst(AuthClaimTypes.SessionId)?.Value;
        return int.TryParse(claim, out int sessionId) ? sessionId : null;
    }

    private async Task PopulateSidebarAsync(CancellationToken cancellationToken)
    {
        ErrorOr<ProfileDto> result = await profileService.GetProfileAsync(cancellationToken);
        if (result.IsError)
        {
            ViewData["ProfileSidebar"] = new ProfileIndexViewModel();
            return;
        }

        ProfileDto profile = result.Value;
        ViewData["ProfileSidebar"] = new ProfileIndexViewModel
        {
            FullName = profile.FullName ?? string.Empty,
            Email = profile.Email ?? string.Empty,
            PhoneNumber = profile.PhoneNumber ?? string.Empty,
            Address = profile.Address ?? string.Empty,
        };
    }
}
