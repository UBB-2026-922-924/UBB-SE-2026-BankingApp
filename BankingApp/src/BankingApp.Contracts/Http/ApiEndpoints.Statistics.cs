namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Statistics
    {
        public const string Base = $"{ApiBase}/statistics";

        public const string SpendingByCategory = "spending-by-category";
        public const string IncomeVsExpenses = "income-vs-expenses";
        public const string BalanceTrends = "balance-trends";
        public const string TopRecipients = "top-recipients";

        public const string SpendingByCategoryFull = $"{Base}/{SpendingByCategory}";
        public const string IncomeVsExpensesFull = $"{Base}/{IncomeVsExpenses}";
        public const string BalanceTrendsFull = $"{Base}/{BalanceTrends}";
        public const string TopRecipientsFull = $"{Base}/{TopRecipients}";
    }
}