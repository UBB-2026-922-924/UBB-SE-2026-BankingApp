namespace BankingApp.Contracts.Features.Statistics.Dtos;

public class CategorySpendingPointDto
{
    public string CategoryName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal ShareOfTotal { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not CategorySpendingPointDto other)
        {
            return false;
        }

        return CategoryName == other.CategoryName
            && Amount == other.Amount
            && ShareOfTotal == other.ShareOfTotal;
    }

    public override int GetHashCode() => HashCode.Combine(CategoryName, Amount, ShareOfTotal);
}
