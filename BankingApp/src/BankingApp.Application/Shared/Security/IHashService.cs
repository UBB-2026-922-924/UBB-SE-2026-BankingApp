namespace BankingApp.Application.Common.Security;

using ErrorOr;

public interface IHashService
{
    public ErrorOr<string> GetHash(string plaintext);
    public ErrorOr<bool> Verify(string plaintext, string hash);
}
