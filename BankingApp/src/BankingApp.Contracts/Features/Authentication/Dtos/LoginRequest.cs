namespace BankingApp.Contracts.Features.Authentication.Dtos;

/// <summary>
///     Represents a login request with email and password credentials.
/// </summary>
public class LoginRequest
{
    /// <summary>
    ///     Gets or sets the email address of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the password of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Password { get; set; } = string.Empty;
}