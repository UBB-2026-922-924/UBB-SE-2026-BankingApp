namespace BankingApp.Contracts.Features.UserProfile.Validation;

using System.Linq;

/// <summary>Provides password validation rules shared across all clients.</summary>
public static class PasswordValidator
{
    /// <summary>The minimum number of characters required for a valid password.</summary>
    public const int MinimumLength = 8;

    /// <summary>Returns <see langword="true"/> when <paramref name="password"/> meets the minimum length requirement.</summary>
    public static bool MeetsMinimumLength(string password)
    {
        return !string.IsNullOrWhiteSpace(password) && password.Length >= MinimumLength;
    }

    /// <summary>
    ///     Returns <see langword="true"/> when <paramref name="password"/> satisfies all strength requirements:
    ///     minimum length, at least one uppercase letter, one lowercase letter, one digit, and one special character.
    /// </summary>
    public static bool IsStrong(string password)
    {
        return MeetsMinimumLength(password)
               && password.Any(char.IsUpper)
               && password.Any(char.IsLower)
               && password.Any(char.IsDigit)
               && password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
