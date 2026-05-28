namespace BankingApp.Web.ViewModels.Shared;

public sealed class PaginationBarViewModel
{
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public string ActionName { get; init; } = string.Empty;
    public string ControllerName { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string?> AdditionalRouteValues { get; init; } =
        new Dictionary<string, string?>();

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public IEnumerable<int> VisiblePageNumbers
    {
        get
        {
            const int windowSize = 5;
            int start = Math.Max(1, CurrentPage - windowSize / 2);
            int end   = Math.Min(TotalPages, start + windowSize - 1);
            start     = Math.Max(1, end - windowSize + 1);
            return Enumerable.Range(start, end - start + 1);
        }
    }
}
