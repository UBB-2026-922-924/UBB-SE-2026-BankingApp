namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Investments
    {
        public const string Base = $"{ApiBase}/investments";

        public const string Portfolio = "portfolio";
        public const string Trade = "trade";
        public const string Logs = "logs";

        public const string PortfolioFull = $"{Base}/{Portfolio}";
        public const string TradeFull = $"{Base}/{Trade}";
        public const string LogsFull = $"{Base}/{Logs}";
    }
}
