namespace BankingApp.Infrastructure.Common.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using Application.Common.Security;
using ErrorOr;
using Microsoft.IdentityModel.Tokens;

/// <summary>
///     Provides JWT generation, validation, and claim extraction using HMAC-SHA256.
/// </summary>
public sealed class JsonWebTokenService : IJsonWebTokenService
{
    private const int TokenExpirationDays = 7;
    private readonly string _secret;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonWebTokenService" /> class.
    /// </summary>
    /// <param name="secret">The symmetric key used for signing tokens.</param>
    public JsonWebTokenService(string secret)
    {
        _secret = secret;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<string> GenerateToken(int userId)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            Claim[] claims = new[] { new Claim("userId", userId.ToString(CultureInfo.InvariantCulture)) };
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(TokenExpirationDays),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception exception)
        {
            return Error.Failure("jwt.generate_failed", exception.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<ClaimsPrincipal> ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var handler = new JwtSecurityTokenHandler();
        try
        {
            ClaimsPrincipal principal = handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = key
                },
                out _);
            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            return Error.Validation("token_expired", "The token has expired.");
        }
        catch (Exception)
        {
            return Error.Validation("token_invalid", "The token signature is invalid or the token is malformed.");
        }
    }

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<int> ExtractUserId(string token)
    {
        ErrorOr<ClaimsPrincipal> principalResult = ValidateToken(token);
        if (principalResult.IsError)
        {
            return principalResult.FirstError;
        }

        Claim? claim = principalResult.Value.FindFirst("userId");
        if (claim is not null && int.TryParse(claim.Value, out int userId))
        {
            return userId;
        }

        return Error.Validation("token_missing_claim", "The token does not contain a valid user ID claim.");
    }
}
