namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Cards
    {
        public const string Base = $"{ApiBase}/cards";

        public const string ById = "{id}";
        public const string Freeze = "{id}/freeze";
        public const string Unfreeze = "{id}/unfreeze";

        public static string ByIdFull(int id) => $"{Base}/{id}";
        public static string FreezeFull(int id) => $"{Base}/{id}/freeze";
        public static string UnfreezeFull(int id) => $"{Base}/{id}/unfreeze";
    }
}
