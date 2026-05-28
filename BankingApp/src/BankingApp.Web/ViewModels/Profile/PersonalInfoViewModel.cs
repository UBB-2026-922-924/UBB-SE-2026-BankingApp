namespace BankingApp.Web.ViewModels.Profile;

using System.ComponentModel.DataAnnotations;

/// <summary>
///     View model for the personal info editing page (GET/POST /Profile/PersonalInfo).
/// </summary>
public class PersonalInfoViewModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [Display(Name = "Full Name")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty; // Not editable typically, but shown

    [Display(Name = "Phone Number")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Address")]
    [MaxLength(200)]
    public string? Address { get; set; }

    /// <summary>
    /// Indicates whether the user has verified their password for the current session to unlock the form.
    /// </summary>
    public bool IsUnlocked { get; set; }

    /// <summary>
    /// Property for binding the unlock password input.
    /// </summary>
    public string? UnlockPassword { get; set; }
}

