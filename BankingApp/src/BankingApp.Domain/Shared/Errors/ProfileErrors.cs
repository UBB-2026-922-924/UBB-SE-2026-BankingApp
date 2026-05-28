namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

/// <summary>
///     Canonical error definitions for profile and registration validation.
/// </summary>
public static class ProfileErrors
{
    /// <summary>A user ID is required but was not supplied.</summary>
    public static readonly Error UserIdRequired =
        Error.Validation("user_id_required", "User ID is required.");

    /// <summary>The full name field is required but was blank.</summary>
    public static readonly Error FullNameRequired =
        Error.Validation("full_name_required", "Full name is required.");

    /// <summary>The supplied phone number is not a valid E.164 number.</summary>
    public static readonly Error InvalidPhone =
        Error.Validation("invalid_phone", "Invalid phone number.");

    /// <summary>The preferred language field is required but was blank.</summary>
    public static readonly Error PreferredLanguageRequired =
        Error.Validation("preferred_language_required", "Preferred language is required.");

    /// <summary>The password does not meet the minimum strength requirements for registration.</summary>
    public static readonly Error WeakPassword =
        Error.Validation(
            "weak_password",
            "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character.");

    /// <summary>The new password does not meet minimum strength requirements for a password change.</summary>
    public static readonly Error WeakPasswordChange =
        Error.Validation(
            "weak_password",
            "Password must contain at least one digit, one uppercase and one special symbol.");

    /// <summary>The supplied current password did not match the stored hash.</summary>
    public static readonly Error IncorrectPassword =
        Error.Validation("incorrect_password", "Current password is incorrect. Please try again.");
}
