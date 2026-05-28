namespace BankingApp.Contracts.Http;

public static class AuthHeaderNames
{
    public const string Authorization = "Authorization";
    public const string BearerScheme = "Bearer";
    public const string BearerPrefix = $"{BearerScheme} ";
}
