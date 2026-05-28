namespace BankingApp.Contracts.Features.UserProfile.Dtos;

/// <summary>
///     Represents a request to update user profile fields.
/// </summary>
public class UpdateProfileRequest
{

    /// <summary>
    ///     Gets or sets the identifier of the user whose profile is being updated.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int? UserId { get; set; }

    /// <summary>
    ///     Gets or sets the new full name.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? FullName { get; set; }

    /// <summary>
    ///     Gets or sets the new phone number.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Gets or sets the new date of birth.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    ///     Gets or sets the new address.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Address { get; set; }

    /// <summary>
    ///     Gets or sets the new nationality.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Nationality { get; set; }

    /// <summary>
    ///     Gets or sets the new preferred language.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? PreferredLanguage { get; set; }
}
