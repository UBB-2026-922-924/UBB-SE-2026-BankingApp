namespace BankingApp.Contracts.Features.Beneficiaries.Dtos;

/// <summary>
///     Represents the data required to update a beneficiary.
/// </summary>
public class UpdateBeneficiaryRequest
{
    /// <summary>
    ///     Gets or sets the beneficiary identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the beneficiary IBAN.
    /// </summary>
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the beneficiary bank name.
    /// </summary>
    public string? BankName { get; set; }
}