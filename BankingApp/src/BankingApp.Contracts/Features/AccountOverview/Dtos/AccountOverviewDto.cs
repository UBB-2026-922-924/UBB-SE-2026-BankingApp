namespace BankingApp.Contracts.Features.AccountOverview.Dtos;

/// <summary>
///     Represents the response containing dashboard data for a user.
/// </summary>
public class AccountOverviewDto
{
    /// <summary>
    ///     Gets or sets the current user information.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public UserSummaryDto? CurrentUser { get; set; }

    /// <summary>
    ///     Gets or sets the list of cards belonging to the user.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public List<CardDto> Cards { get; set; } = [];

    /// <summary>
    ///     Gets or sets the list of recent transactions.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public List<TransactionDto> RecentTransactions { get; set; } = [];

    /// <summary>
    ///     Gets or sets the count of unread notifications.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UnreadNotificationCount { get; set; }

    /// <summary>
    ///     Gets or sets the list of pending transfers for the user.
    /// </summary>
    /// <value>
    ///     A list of transactions representing pending transfers.
    /// </value>
    public List<TransactionDto> PendingTransfers { get; set; } = [];
}
