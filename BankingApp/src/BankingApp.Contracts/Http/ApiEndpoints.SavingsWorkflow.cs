namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class SavingsWorkflow
    {
        public const string Base = $"{ApiBase}/savings-workflow";

        public const string WithdrawResultMessage = "withdraw-result-message";
        public const string CanMoveNext = "can-move-next";
        public const string CanMovePrevious = "can-move-previous";
        public const string DefaultCloseDestination = "default-close-destination";
        public const string DefaultFundingSource = "default-funding-source";
        public const string ValidateClose = "validate-close";
        public const string ValidateWithdraw = "validate-withdraw";

        public const string WithdrawResultMessageFull = $"{Base}/{WithdrawResultMessage}";
        public const string CanMoveNextFull = $"{Base}/{CanMoveNext}";
        public const string CanMovePreviousFull = $"{Base}/{CanMovePrevious}";
        public const string DefaultCloseDestinationFull = $"{Base}/{DefaultCloseDestination}";
        public const string DefaultFundingSourceFull = $"{Base}/{DefaultFundingSource}";
        public const string ValidateCloseFull = $"{Base}/{ValidateClose}";
        public const string ValidateWithdrawFull = $"{Base}/{ValidateWithdraw}";
    }
}
