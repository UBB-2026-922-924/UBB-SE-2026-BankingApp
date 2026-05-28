namespace BankingApp.Contracts.Features.AccountOverview.Dtos;

/// <summary>
///     Data transfer object containing the user information shown on the dashboard.
/// </summary>
public class UserSummaryDto
{
    /// <summary>
    ///     Gets or sets the full name of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the email address of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the phone number of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? PhoneNumber { get; set; }

}
