namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

/// <summary>
///     Canonical error definitions for user management and profile updates.
/// </summary>
public static class UserErrors
{
    /// <summary>The user account could not be created in the data store.</summary>
    public static readonly Error UserCreationFailed =
        Error.Failure("user_creation_failed", "Failed to create user account.");

    /// <summary>The newly created user could not be retrieved after creation.</summary>
    public static readonly Error UserRetrievalFailed =
        Error.Failure("user_retrieval_failed", "Failed to retrieve created user.");

    /// <summary>A login session could not be created for the user.</summary>
    public static readonly Error SessionCreationFailed =
        Error.Failure("session_failed", "Failed to create session.");

    /// <summary>The user record could not be updated.</summary>
    public static readonly Error UpdateFailed =
        Error.Failure("update_failed", "Could not update user.");

    /// <summary>The password hash could not be persisted.</summary>
    public static readonly Error PasswordUpdateFailed =
        Error.Failure("update_failed", "Could not update password. Please try again.");

    /// <summary>The notification preferences could not be saved.</summary>
    public static readonly Error NotificationPreferencesUpdateFailed =
        Error.Failure("update_failed", "Failed to update notification preferences.");

    /// <summary>A transient database error prevented the operation from completing.</summary>
    public static readonly Error DatabaseError =
        Error.Failure("database_error", "A service error occurred. Please try again later.");

    /// <summary>The provided email address is not a valid email format.</summary>
    public static readonly Error InvalidEmail =
        Error.Validation("user.invalid_email", "The email address is not valid.");

    /// <summary>No user account exists for the given identifier.</summary>
    public static readonly Error NotFound =
        Error.NotFound("user.not_found", "User was not found.");
}
