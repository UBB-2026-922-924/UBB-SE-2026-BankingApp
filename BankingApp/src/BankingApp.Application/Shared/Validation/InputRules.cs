namespace BankingApp.Application.Common.Validation;

using EmailValidation;
using PhoneNumbers;

/// <summary>
///     Provides common input validation helper methods.
/// </summary>
public static class InputRules
{
    private const int MinPasswordLength = 8;

    /// <summary>
    ///     Determines whether the specified string is a valid email address.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    /// <returns><see langword="true" /> if the email is valid; otherwise, <see langword="false" />.</returns>
    public static bool IsValidEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && EmailValidator.Validate(email.Trim());
    }

    /// <summary>
    ///     Determines whether the specified password meets strength requirements
    ///     (at least 8 characters, with uppercase, lowercase, digit, and special character).
    /// </summary>
    /// <param name="password">The password to evaluate.</param>
    /// <returns><see langword="true" /> if the password is strong; otherwise, <see langword="false" />.</returns>
    public static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        return password.Length >= MinPasswordLength
               && password.Any(char.IsUpper)
               && password.Any(char.IsLower)
               && password.Any(char.IsDigit)
               && password.Any(character => !char.IsLetterOrDigit(character));
    }

    /// <summary>
    ///     Determines whether the specified string is a valid international phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number string to validate.</param>
    /// <returns><see langword="true" /> if the number is valid; otherwise, <see langword="false" />.</returns>
    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        try
        {
            var phoneUtil = PhoneNumberUtil.GetInstance();
            PhoneNumber parsed = phoneUtil.Parse(phoneNumber, null);
            return phoneUtil.IsValidNumber(parsed);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Normalizes a phone number to E.164 format (e.g. +40712345678).
    /// </summary>
    /// <param name="phoneNumber">The phone number to normalize.</param>
    /// <param name="defaultRegion">The default region code (e.g. "RO", "US"). Defaults to "RO".</param>
    /// <returns>The E.164 formatted number, or <see langword="null" /> if the number is invalid.</returns>
    public static string? NormalizePhoneNumber(string? phoneNumber, string defaultRegion = "RO")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        try
        {
            var phoneUtil = PhoneNumberUtil.GetInstance();
            PhoneNumber parsed = phoneUtil.Parse(phoneNumber, defaultRegion);
            if (!phoneUtil.IsValidNumber(parsed))
            {
                return null;
            }

            return phoneUtil.Format(parsed, PhoneNumberFormat.E164);
        }
        catch (NumberParseException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Determines whether two password strings are equal.
    /// </summary>
    /// <param name="firstPassword">The first password.</param>
    /// <param name="secondPassword">The second password.</param>
    /// <returns><see langword="true" /> if both passwords are non-null and equal; otherwise, <see langword="false" />.</returns>
    public static bool PasswordsMatch(string? firstPassword, string? secondPassword)
    {
        if (firstPassword == null || secondPassword == null)
        {
            return false;
        }

        return firstPassword == secondPassword;
    }

}
