namespace BankingApp.Web.Models.Transfers;

/// <summary>
///     View model for the transfer history page (GET /Transfers/History).
/// </summary>
public class TransferHistoryModel
{
    /// <summary>Gets or sets the list of past transfers.</summary>
    public List<TransferHistoryRowModel> Transfers { get; set; } = [];

    /// <summary>Gets a value indicating whether there are any transfers to display.</summary>
    public bool HasTransfers => Transfers.Count > 0;
}
