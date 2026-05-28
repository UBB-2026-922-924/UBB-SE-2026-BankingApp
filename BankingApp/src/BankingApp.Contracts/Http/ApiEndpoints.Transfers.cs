namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Transfers
    {
        public const string Base = $"{ApiBase}/transfers";
        public const string LegacyBase = $"{ApiBase}/transfer";

        public const string Accounts = "accounts";
        public const string ValidateIban = "validate-iban";
        public const string FxPreview = "fx-preview";
        public const string Execute = "execute";

        public const string AccountsFull = $"{Base}/{Accounts}";
        public const string ValidateIbanFull = $"{Base}/{ValidateIban}";
        public const string FxPreviewFull = $"{Base}/{FxPreview}";
        public const string ExecuteFull = $"{Base}/{Execute}";
    }
}
