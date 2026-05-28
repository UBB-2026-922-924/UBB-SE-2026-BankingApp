namespace BankingApp.Infrastructure.Common.Security;

using Application.Common.Security;
using ErrorOr;

/// <summary>
///     Provides BCrypt-based password hashing and verification.
/// </summary>
public sealed class HashService : IHashService
{
    /// <inheritdoc />
    /// <param name="plaintext">The plaintext value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<string> GetHash(string plaintext)
    {
        try
        {
            return BCrypt.Net.BCrypt.HashPassword(plaintext);
        }
        catch (Exception exception)
        {
            return Error.Failure("hash.failed", exception.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="plaintext">The plaintext value.</param>
    /// <param name="hash">The hash value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<bool> Verify(string plaintext, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(plaintext, hash);
        }
        catch (Exception exception)
        {
            return Error.Failure("hash.verify_failed", exception.Message);
        }
    }
}
