using BankingApp.Domain.Aggregates.SavingsAggregate;

namespace BankingApp.Contracts.Features.Savings.Dtos
{
    public class SavingsAccountSnapshotDto
    {
        public int IdentificationNumber { get; set; }

        public decimal Balance { get; set; }

        public decimal AnnualPercentageYield { get; set; }

        public string SavingsType { get; set; } = string.Empty;

        public DateTime? MaturityDate { get; set; }

        public static SavingsAccountSnapshotDto FromAccount(SavingsAccount account)
        {
            return new SavingsAccountSnapshotDto
            {
                IdentificationNumber = account.IdentificationNumber,
                Balance = account.Balance,
                AnnualPercentageYield = account.AnnualPercentageYield,
                SavingsType = account.SavingsType,
                MaturityDate = account.MaturityDate,
            };
        }
    }
}
