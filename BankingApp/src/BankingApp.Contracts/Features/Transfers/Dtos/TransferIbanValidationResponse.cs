namespace BankingApp.Contracts.Features.Transfers.Dtos;

/// <summary>
///     Represents the result of validating a transfer recipient IBAN.
/// </summary>
public class TransferIbanValidationResponse
{
    /// <summary>Gets or sets a value indicating whether the IBAN is valid.</summary>
    public bool IsValid { get; set; }

    /// <summary>Gets or sets the inferred bank name.</summary>
    public string BankName { get; set; } = string.Empty;
}
