namespace BankingApp.Contracts.Features.Savings.Dtos
{
    public class ValidationResponse
    {
        public bool IsValid { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
