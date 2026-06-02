namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class SavingsUiRules
    {
        public const string Base = $"{ApiBase}/savings-ui-rules";

        public const string DepositPreview = "deposit-preview";
        public const string TotalPages = "total-pages";
        public const string WithdrawNetAmount = "withdraw-net-amount";
        public const string ParseDepositFrequency = "parse-deposit-frequency";
        public const string ParsePositiveAmount = "parse-positive-amount";
        public const string ValidateCreateAccount = "validate-create-account";

        public const string DepositPreviewFull = $"{Base}/{DepositPreview}";
        public const string TotalPagesFull = $"{Base}/{TotalPages}";
        public const string WithdrawNetAmountFull = $"{Base}/{WithdrawNetAmount}";
        public const string ParseDepositFrequencyFull = $"{Base}/{ParseDepositFrequency}";
        public const string ParsePositiveAmountFull = $"{Base}/{ParsePositiveAmount}";
        public const string ValidateCreateAccountFull = $"{Base}/{ValidateCreateAccount}";
    }
}
