namespace BankingApp.Infrastructure.Persistence.Tests.Common.Security;

public sealed class JsonWebTokenServiceTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void GenerateToken_WhenSecretIsValid_ShouldReturnNonEmptyToken()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void ValidateToken_WhenTokenIsValid_ShouldReturnClaimsPrincipal()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void ValidateToken_WhenTokenIsExpired_ShouldReturnValidationError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void ValidateToken_WhenTokenIsMalformed_ShouldReturnValidationError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void ExtractUserId_WhenTokenContainsUserIdClaim_ShouldReturnUserId()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void ExtractUserId_WhenTokenIsInvalid_ShouldReturnError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void ExtractUserId_WhenTokenMissingUserIdClaim_ShouldReturnError()
    {
        throw new NotImplementedException();
    }
}
