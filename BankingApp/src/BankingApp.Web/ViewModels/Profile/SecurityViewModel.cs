namespace BankingApp.Web.ViewModels.Profile;

using System.ComponentModel.DataAnnotations;
using BankingApp.Contracts.Features.UserProfile.Validation;

/// <summary>
///     Legacy view model kept for backward compatibility.
///     The two-step flow uses <see cref="VerifyPasswordViewModel"/> and <see cref="ChangePasswordViewModel"/> instead.
/// </summary>
public sealed class SecurityViewModel
{
    [Required(ErrorMessage = "Current password is required.")]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [Display(Name = "New password")]
    [MinLength(PasswordValidator.MinimumLength, ErrorMessage = "Password must be at least 8 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm new password")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
