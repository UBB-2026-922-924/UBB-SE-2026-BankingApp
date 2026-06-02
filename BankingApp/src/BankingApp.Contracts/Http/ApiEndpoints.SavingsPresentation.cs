namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class SavingsPresentation
    {
        public const string Base = $"{ApiBase}/savings-presentation";

        public const string ClosePenaltyRisk = "close-penalty-risk";
        public const string BestInterestRate = "best-interest-rate";
        public const string AccountsText = "accounts-text";
        public const string TotalSaved = "total-saved";

        public const string ClosePenaltyRiskFull = $"{Base}/{ClosePenaltyRisk}";
        public const string BestInterestRateFull = $"{Base}/{BestInterestRate}";
        public const string TotalSavedFull = $"{Base}/{TotalSaved}";

        public static string AccountsTextFull(int accountCount) => $"{Base}/{AccountsText}/{accountCount}";
    }
}
