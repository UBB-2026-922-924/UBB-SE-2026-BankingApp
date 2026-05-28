namespace BankingApp.Contracts.Features.Billers.Dtos;

/// <summary>
///     Represents a biller in API responses.
/// </summary>
public class BillerDto
{
    /// <summary>Gets or sets the unique identifier for the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the display name of the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category of the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL of the biller logo.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? LogoUrl { get; set; }

    /// <summary>Gets or sets a value indicating whether the biller is active.</summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsActive { get; set; }
}
