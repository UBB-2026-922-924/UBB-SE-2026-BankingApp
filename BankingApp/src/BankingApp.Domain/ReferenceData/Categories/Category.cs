namespace BankingApp.Domain.ReferenceData.Categories;

/// <summary>
///     Represents shared reference data for transaction categorization.
/// </summary>
public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public bool IsSystem { get; set; } = true;
}
