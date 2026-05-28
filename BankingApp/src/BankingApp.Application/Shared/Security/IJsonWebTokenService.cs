namespace BankingApp.Application.Common.Security;

using ErrorOr;

public interface IJsonWebTokenService
{
    public ErrorOr<string> GenerateToken(int userId);
    public ErrorOr<int> ExtractUserId(string token);
}
