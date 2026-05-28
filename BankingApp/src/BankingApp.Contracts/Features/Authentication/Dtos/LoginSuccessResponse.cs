namespace BankingApp.Contracts.Features.Authentication.Dtos;

/// <summary>
///     The JSON response body returned by a successful login endpoint.
/// </summary>
public sealed class LoginSuccessResponse
{
    /// <summary>
    ///     Gets or sets the identifier of the authenticated user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the signed JWT for subsequent authenticated requests.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Token { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the session opened on this login.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int? SessionId { get; set; }
}
