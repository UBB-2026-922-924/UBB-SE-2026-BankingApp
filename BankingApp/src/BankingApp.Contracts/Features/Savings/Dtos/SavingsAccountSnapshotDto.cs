namespace BankingApp.Contracts.Features.Savings.Dtos;

using Domain.Aggregates.SavingsAggregate;

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
            IdentificationNumber = account.Id,
            Balance = account.Balance,
            AnnualPercentageYield = account.AnnualPercentageYield,
            SavingsType = account.SavingsType.ToString(),
            MaturityDate = account.MaturityDate,
        };
    }
}
