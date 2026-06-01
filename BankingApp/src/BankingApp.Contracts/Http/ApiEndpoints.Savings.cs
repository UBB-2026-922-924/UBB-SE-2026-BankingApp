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
    }
}
