namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Savings
    {
        public const string Base = $"{ApiBase}/savings";

        public const string Accounts = "accounts";
        public const string Deposit = "{accountId:int}/deposit";
        public const string Withdraw = "{accountId:int}/withdraw";
        public const string Close = "{accountId:int}/close";
        public const string AutoDepositByAccount = "{accountId:int}/auto-deposit";
        public const string AutoDeposit = "auto-deposit";
        public const string FundingSources = "funding-sources";
        public const string Transactions = "{accountId:int}/transactions";
        public const string ValidDestinations = "{currentAccountId:int}/valid-destinations";

        public const string AccountsFull = $"{Base}/{Accounts}";
        public const string AutoDepositFull = $"{Base}/{AutoDeposit}";
        public const string FundingSourcesFull = $"{Base}/{FundingSources}";

        public static string DepositFull(int accountId) => $"{Base}/{accountId}/deposit";

        public static string WithdrawFull(int accountId) => $"{Base}/{accountId}/withdraw";

        public static string CloseFull(int accountId) => $"{Base}/{accountId}/close";

        public static string AutoDepositByAccountFull(int accountId) => $"{Base}/{accountId}/auto-deposit";

        public static string TransactionsFull(int accountId) => $"{Base}/{accountId}/transactions";

        public static string ValidDestinationsFull(int currentAccountId) => $"{Base}/{currentAccountId}/valid-destinations";

        public static string PenaltyRateFull(string penaltyCase) => $"{Base}/penalty/rate/{penaltyCase}";
    }
}
