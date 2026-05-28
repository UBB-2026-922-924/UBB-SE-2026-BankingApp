namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

/// <summary>
///     Canonical error definitions for authentication and login flows.
/// </summary>
public static class AuthErrors
{
    /// <summary>The provided email address has an invalid format.</summary>
    public static readonly Error InvalidEmail =
        Error.Validation("invalid_email", "Invalid email format.");

    /// <summary>The email/password combination did not match any account.</summary>
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("invalid_credentials", "Invalid email or password.");

    /// <summary>The account was locked after too many consecutive failures.</summary>
    public static readonly Error TooManyFailedAttempts =
        Error.Unauthorized("invalid_credentials", "Too many failed attempts. Please try again later.");

    /// <summary>The account is temporarily locked; the lockout window is still active.</summary>
    public static readonly Error AccountLocked =
        Error.Forbidden("account_locked", "Account is locked. Try again later.");

    /// <summary>The account was just locked because the maximum failed-attempt threshold was reached.</summary>
    public static readonly Error AccountLockedTooManyAttempts =
        Error.Forbidden("account_locked", "Account locked due to too many failed attempts.");

    /// <summary>No user account exists for the given identifier.</summary>
    public static readonly Error UserNotFound =
        Error.NotFound("user_not_found", "User not found.");

    /// <summary>An account with the given email address already exists.</summary>
    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("email_registered", "Email is already registered.");

    /// <summary>The session token was not found or has already been revoked.</summary>
    public static readonly Error SessionNotFound =
        Error.NotFound("session_not_found", "Session not found.");

}
