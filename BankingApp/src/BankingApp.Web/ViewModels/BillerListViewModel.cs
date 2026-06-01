namespace BankingApp.Web.ViewModels;

public sealed class BillerListViewModel
{
    public IList<BillerRow> Billers { get; init; } = [];

    public sealed class BillerRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
        public bool IsSaved { get; init; }
    }
}