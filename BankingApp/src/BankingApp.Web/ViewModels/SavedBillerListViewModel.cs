namespace BankingApp.Web.ViewModels;

public sealed class SavedBillerListViewModel
{
    public IList<SavedBillerRow> SavedBillers { get; init; } = [];

    public sealed class SavedBillerRow
    {
        public int Id { get; init; }
        public int BillerId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
        public string? DefaultReference { get; init; }
    }
}