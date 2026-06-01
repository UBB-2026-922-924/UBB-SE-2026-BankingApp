namespace BankingApp.Web.Models.Dashboard;

using Contracts.Features.AccountOverview.Dtos;

/// <summary>Top-level model for the Dashboard index page.</summary>
public class DashboardModel
{
    /// <summary>Gets or sets the current user summary.</summary>
    public UserSummaryDto UserSummary { get; init; } = new();

    /// <summary>Gets or sets the list of view-ready card models.</summary>
    public IReadOnlyList<DashboardCardModel> Cards { get; init; } = [];

    /// <summary>Gets a value indicating whether the user has any cards.</summary>
    public bool HasCards => Cards.Count > 0;

    /// <summary>Gets or sets the list of view-ready recent transaction models.</summary>
    public IReadOnlyList<DashboardTransactionModel> RecentTransactions { get; init; } = [];

    /// <summary>Gets a value indicating whether there are any recent transactions.</summary>
    public bool HasTransactions => RecentTransactions.Count > 0;

    /// <summary>Gets or sets the count of unread notifications for the current user.</summary>
    public int UnreadNotificationCount { get; init; }
}
