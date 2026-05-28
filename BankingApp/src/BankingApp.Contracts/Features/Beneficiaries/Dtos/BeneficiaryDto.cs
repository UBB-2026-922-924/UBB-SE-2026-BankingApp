namespace BankingApp.Contracts.Features.Beneficiaries.Dtos;

/// <summary>
///     Represents a beneficiary saved by a user for future transfers.
/// </summary>
public class BeneficiaryDto
{
    /// <summary>
    ///     Gets or sets the beneficiary identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the user identifier who owns this beneficiary.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary IBAN.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary bank name.
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    ///     Gets or sets the date of the last transfer made to the beneficiary.
    /// </summary>
    public DateTime? LastTransferDate { get; set; }

    /// <summary>
    ///     Gets or sets the total amount historically sent to the beneficiary.
    /// </summary>
    public decimal TotalAmountSent { get; set; }

    /// <summary>
    ///     Gets or sets the number of transfers made to the beneficiary.
    /// </summary>
    public int TransferCount { get; set; }

    /// <summary>
    ///     Gets or sets the UTC timestamp when the beneficiary was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
