namespace BankingApp.Contracts.Http;

public static partial class ApiEndpoints
{
    public static class Loans
    {
        public const string Base = $"{ApiBase}/loans";

        public const string ById = "{id:int}";
        public const string Applications = "applications";
        public const string Estimate = "estimate";
        public const string PayInstallment = "{loanId:int}/pay-installment";
        public const string AmortizationSchedule = "{loanId:int}/amortization-schedule";

        public const string ApplicationsFull = $"{Base}/{Applications}";
        public const string EstimateFull = $"{Base}/{Estimate}";
    }
}
