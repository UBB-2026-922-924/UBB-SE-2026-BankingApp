namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class BillPayments
    {
        public const string Base = $"{ApiBase}/bill-payment";

        public const string Accounts = "accounts";
        public const string Fee = "fee";
        public const string Pay = "pay";
        public const string History = "history";

        public const string AccountsFull = $"{Base}/{Accounts}";
        public const string FeeFull = $"{Base}/{Fee}";
        public const string PayFull = $"{Base}/{Pay}";
        public const string HistoryFull = $"{Base}/{History}";
    }
}
