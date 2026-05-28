namespace BankingApp.Desktop.Shared.Validation;

using SharedValidator = BankingApp.Contracts.Features.UserProfile.Validation.PasswordValidator;

/// <summary>Provides password validation rules shared across client registration and security flows.</summary>
/// <remarks>Delegates to the shared <see cref="SharedValidator"/> in the Contracts layer.</remarks>
public static class PasswordValidator
{
    /// <summary>The minimum number of characters required for a valid password.</summary>
    public const int MinimumLength = SharedValidator.MinimumLength;

    /// <summary>Returns <see langword="true"/> when <paramref name="password"/> meets the minimum length requirement.</summary>
    public static bool MeetsMinimumLength(string password) => SharedValidator.MeetsMinimumLength(password);

    /// <summary>
    ///     Returns <see langword="true"/> when <paramref name="password"/> satisfies all strength requirements:
    ///     minimum length, at least one uppercase letter, one lowercase letter, one digit, and one special character.
    /// </summary>
    public static bool IsStrong(string password) => SharedValidator.IsStrong(password);
}
