namespace BankingApp.Api.Configuration;

// Awful naming, but for backwards compatibility with what the others will move too...
public class TeamCOptions
{
    public const string SectionName = "TeamC";

    public int CardRevealDurationSeconds { get; set; } = 30;
    public decimal MaximumSpendingLimit { get; set; } = 100000m;
    public int BalanceTrendDays { get; set; } = 30;
    public int TopRecipientsCount { get; set; } = 5;
}

