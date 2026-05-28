namespace BankingApp.Web.Models.BillPayments;

/// <summary>
///     Model for the bill-payment history page (History view).
/// </summary>
public class BillPaymentHistoryModel
{
    /// <summary>Gets or sets the list of past bill payments, newest first.</summary>
    public List<BillPaymentRowModel> Payments { get; set; } = [];

    /// <summary>Gets a value indicating whether there are any payments to display.</summary>
    public bool HasPayments => Payments.Count > 0;
}
