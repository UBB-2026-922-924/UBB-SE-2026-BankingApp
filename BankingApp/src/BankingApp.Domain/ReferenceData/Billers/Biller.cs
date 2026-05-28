namespace BankingApp.Domain.ReferenceData.Billers;

using Enums;

/// <summary>
/// Represents shared reference data for a registered biller.
/// </summary>
public class Biller
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public BillerCategory Category { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
