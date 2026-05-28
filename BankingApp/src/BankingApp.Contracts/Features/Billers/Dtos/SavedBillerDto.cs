namespace BankingApp.Contracts.Features.Billers.Dtos;

/// <summary>
///     Represents a saved biller entry in API responses.
/// </summary>
public class SavedBillerDto
{
    /// <summary>Gets or sets the unique identifier for the saved biller entry.</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the owning user identifier.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the identifier of the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the name of the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string BillerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the category of the biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string BillerCategory { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL of the biller logo.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? LogoUrl { get; set; }

    /// <summary>Gets or sets the user-assigned nickname for this biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? Nickname { get; set; }

    /// <summary>Gets or sets the default payment reference for this biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? DefaultReference { get; set; }

    /// <summary>Gets or sets the date and time the biller was saved.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the associated biller details.</summary>
    /// <value>Gets or sets the current value.</value>
    public BillerDto? Biller { get; set; }

    /// <summary>Gets the display name for the saved biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string DisplayName => string.IsNullOrWhiteSpace(Nickname) ? BillerName : Nickname;

    /// <summary>Gets the category display text for the saved biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public string DisplayCategory => Biller?.Category ?? BillerCategory;

    /// <summary>Converts the saved biller into a biller selection.</summary>
    /// <returns>The biller DTO.</returns>
    public BillerDto ToBiller()
    {
        return Biller ?? new BillerDto
        {
            Id = BillerId,
            Name = BillerName,
            Category = BillerCategory,
            LogoUrl = LogoUrl,
            IsActive = true
        };
    }
}
