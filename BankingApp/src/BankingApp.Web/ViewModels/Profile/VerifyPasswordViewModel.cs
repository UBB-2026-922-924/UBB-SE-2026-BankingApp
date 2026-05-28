namespace BankingApp.Web.ViewModels.Profile;

using System.ComponentModel.DataAnnotations;

/// <summary>
///     View model for step one of the two-step security flow: verify current password.
/// </summary>
public sealed class VerifyPasswordViewModel
{
    [Required(ErrorMessage = "Current password is required.")]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;
}
