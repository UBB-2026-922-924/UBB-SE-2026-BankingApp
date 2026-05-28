namespace BankingApp.Contracts.Features.BillPayments.Dtos;

/// <summary>
///     Represents the fee calculation response.
/// </summary>
public class FeeResponse
{
    /// <summary>
    ///     Gets or sets the calculated fee.
    /// </summary>
    /// <value>The fee amount.</value>
    public decimal Fee { get; set; }
}
