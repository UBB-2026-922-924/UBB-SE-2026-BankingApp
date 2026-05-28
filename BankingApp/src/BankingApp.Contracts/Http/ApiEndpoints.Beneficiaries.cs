namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Beneficiaries
    {
        public const string Base = $"{ApiBase}/beneficiaries";
        public const string ById = "{id:int}";

        public static string ByIdFull(int id) => $"{Base}/{id}";
    }
}
