namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Profile
    {
        public const string Base = $"{ApiBase}/profile";

        public const string ChangePassword = "password";
        public const string NotificationPreferences = "notifications/preferences";
        public const string VerifyPassword = "verify-password";
        public const string Sessions = "sessions";
        public const string SessionById = "sessions/{sessionId:int}";

        public const string ChangePasswordFull = $"{Base}/{ChangePassword}";
        public const string NotificationPreferencesFull = $"{Base}/{NotificationPreferences}";
        public const string VerifyPasswordFull = $"{Base}/{VerifyPassword}";
        public const string SessionsFull = $"{Base}/{Sessions}";

        public static string SessionByIdFull(int sessionId) => $"{SessionsFull}/{sessionId}";
    }
}
