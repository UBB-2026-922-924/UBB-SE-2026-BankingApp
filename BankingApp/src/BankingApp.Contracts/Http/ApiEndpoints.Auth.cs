namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Auth
    {
        public const string Base = $"{ApiBase}/auth";

        public const string Login = "login";
        public const string Register = "register";
        public const string Logout = "logout";

        public const string LoginFull = $"{Base}/{Login}";
        public const string RegisterFull = $"{Base}/{Register}";
        public const string LogoutFull = $"{Base}/{Logout}";
    }
}
