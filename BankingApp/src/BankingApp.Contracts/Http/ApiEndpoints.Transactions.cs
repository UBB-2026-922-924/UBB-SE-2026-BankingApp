namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Transactions
    {
        public const string Base = $"{ApiBase}/transactions";

        public const string History = "history";
        public const string Filters = "filters";
        public const string Export = "export";
        public const string ById = "{transactionId:int}";
        public const string Receipt = "{transactionId:int}/receipt";

        public static string ByIdFull(int id) => $"{Base}/{id}";
        public static string ByReceiptFull(int id) => $"{Base}/{id}/receipt";
    }
}