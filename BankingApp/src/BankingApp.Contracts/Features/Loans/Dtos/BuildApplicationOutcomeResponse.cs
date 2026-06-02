namespace BankingApp.Contracts.Features.Loans.Dtos
{
    public class BuildApplicationOutcomeResponse
    {
        public bool IsApproved { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
