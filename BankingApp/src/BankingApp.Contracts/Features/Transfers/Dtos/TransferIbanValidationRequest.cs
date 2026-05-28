namespace BankingApp.Contracts.Features.Transfers.Dtos;

/// <summary>
///     Represents the request payload used to validate a recipient IBAN.
/// </summary>
public class TransferIbanValidationRequest
{
    /// <summary>Gets or sets the IBAN to validate.</summary>
    public string Iban { get; set; } = string.Empty;
}
