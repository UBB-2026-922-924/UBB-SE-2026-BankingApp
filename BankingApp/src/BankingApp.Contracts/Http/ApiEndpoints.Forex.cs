namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Forex
    {
        public const string Base = $"{ApiBase}/forex";

        public const string Preview = "preview";
        public const string Execute = "execute";
        public const string History = "history";

        public const string PreviewFull = $"{Base}/{Preview}";
        public const string ExecuteFull = $"{Base}/{Execute}";
        public const string HistoryFull = $"{Base}/{History}";
    }
}
