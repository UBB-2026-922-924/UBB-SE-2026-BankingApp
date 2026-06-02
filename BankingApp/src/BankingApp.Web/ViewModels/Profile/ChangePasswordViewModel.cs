namespace BankingApp.Web.ViewModels.Profile;

using System.ComponentModel.DataAnnotations;
using Contracts.Features.UserProfile.Validation;

/// <summary>
///     View model for step two of the two-step security flow: set new password.
/// </summary>
public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "New password is required.")]
    [Display(Name = "New password")]
    [MinLength(PasswordValidator.MinimumLength, ErrorMessage = "Password must be at least 8 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm new password")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
