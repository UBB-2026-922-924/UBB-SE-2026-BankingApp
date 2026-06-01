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

        public static string ByIdFull(int id) => $"{Base}/{id}";

        public static string ApplicationStatusFull(int applicationId) => $"{ApplicationsFull}/{applicationId}/status";

        public static string AfterPaymentFull(int loanId) => $"{Base}/{loanId}/after-payment";

        public static string AmortizationScheduleFull(int loanId) => $"{Base}/{loanId}/amortization-schedule";
    }

    public static class LoanDialogState
    {
        public const string Base = $"{Loans.Base}/should-compute-estimate";
    }

    public static class LoanApplicationPresentation
    {
        public const string Base = $"{Loans.Base}/loan-application-presentation-outcome";
    }

    public static class LoanPresentation
    {
        public const string Base = $"{Loans.Base}/repayment-progress";
    }
}
