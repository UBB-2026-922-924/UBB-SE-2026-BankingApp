namespace BankingApp.Contracts.Features.Authentication.Dtos;

/// <summary>
///     Represents a completed login where a JWT has been issued and the session is active.
/// </summary>
public sealed class LoginSuccess
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginSuccess" /> class.
    /// </summary>
    /// <param name="userId">The identifier of the authenticated user.</param>
    /// <returns>The result of the operation.</returns>
    /// <param name="token">The signed JWT for subsequent authenticated requests.</param>
    /// <param name="sessionId">The identifier of the opened session.</param>
    public LoginSuccess(int userId, string token, int sessionId)
    {
        UserId = userId;
        Token = token;
        SessionId = sessionId;
    }

    /// <summary>
    ///     Gets the identifier of the authenticated user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; }

    /// <summary>
    ///     Gets the signed JWT for subsequent authenticated requests.
    /// </summary>
    /// <value>
    ///     Gets the current value.
    /// </value>
    public string Token { get; }

    /// <summary>
    ///     Gets the identifier of the session opened on this login.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int SessionId { get; }
}
