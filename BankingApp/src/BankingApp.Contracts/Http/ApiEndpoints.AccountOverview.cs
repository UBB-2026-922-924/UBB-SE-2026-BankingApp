namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class AccountOverview
    {
        public const string Base = $"{ApiBase}/account-overview";

        public const string Accounts = "accounts";
        public const string AccountsFull = $"{ApiBase}/{Base}/{Accounts}";
    }
}
