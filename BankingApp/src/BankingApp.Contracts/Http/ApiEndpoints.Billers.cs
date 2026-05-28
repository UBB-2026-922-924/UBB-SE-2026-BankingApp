namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Billers
    {
        public const string Base = $"{ApiBase}/billers";

        public const string Saved = "saved";
        public const string SavedById = "saved/{id:int}";

        public const string SavedFull = $"{Base}/{Saved}";

        public static string SavedByIdFull(int id) => $"{SavedFull}/{id}";
    }
}
