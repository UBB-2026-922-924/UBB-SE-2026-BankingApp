namespace BankingApp.Web.Models.Forex;

using System.Collections.Generic;

/// <summary>
///     Model for the forex transaction history page (GET /Forex/History).
/// </summary>
public class ForexHistoryModel
{
    /// <summary>Gets or sets the list of past forex transactions, newest first.</summary>
    public List<ForexHistoryRowModel> Transactions { get; set; } = [];

    /// <summary>Gets a value indicating whether there are any transactions to display.</summary>
    public bool HasTransactions => Transactions.Count > 0;
}
