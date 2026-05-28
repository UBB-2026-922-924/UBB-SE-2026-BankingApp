namespace BankingApp.Web.ViewModels.Profile;

/// <summary>
///     View model for the user profile index page (GET /Profile).
/// </summary>
public class ProfileIndexViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

