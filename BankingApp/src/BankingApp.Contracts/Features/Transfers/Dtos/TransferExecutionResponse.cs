namespace BankingApp.Contracts.Features.Transfers.Dtos;

/// <summary>
///     Represents the compact transfer execution response returned to the desktop wizard.
/// </summary>
public class TransferExecutionResponse
{
    /// <summary>Gets or sets the transaction reference.</summary>
    public string TransactionRef { get; set; } = string.Empty;
}
