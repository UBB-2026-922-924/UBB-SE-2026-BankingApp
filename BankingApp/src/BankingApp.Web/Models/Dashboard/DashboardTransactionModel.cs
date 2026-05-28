namespace BankingApp.Web.Models.Dashboard;

using System.Globalization;
using BankingApp.Contracts.Features.AccountOverview.Dtos;
using BankingApp.Domain.Enums;

/// <summary>Model for a single transaction row displayed on the dashboard.</summary>
public class DashboardTransactionModel
{
    /// <summary>Gets the merchant / counterparty display name.</summary>
    public string MerchantDisplay { get; }

    /// <summary>Gets the amount formatted with a leading +/- sign (e.g. "+1,234.56").</summary>
    public string AmountDisplay { get; }

    /// <summary>Gets the currency code (e.g. "USD").</summary>
    public string Currency { get; }

    /// <summary>Gets the transaction date formatted as "dd MMM yyyy".</summary>
    public string Date { get; }

    /// <summary>Gets a value indicating whether this is an inbound (credit) transaction.</summary>
    public bool IsCredit { get; }

    /// <summary>Gets the Bootstrap badge CSS class for the transaction status.</summary>
    public string StatusBadgeClass { get; }

    /// <summary>Gets the transaction status display label.</summary>
    public string StatusDisplay { get; }

    /// <summary>Initialises a new <see cref="DashboardTransactionModel"/> from a <see cref="TransactionDto"/>.</summary>
    public DashboardTransactionModel(TransactionDto dto)
    {
        MerchantDisplay = FirstNonEmpty(
            dto.MerchantName,
            dto.Description,
            dto.CounterpartyName,
            "Transaction");

        IsCredit = dto.Direction == TransactionDirection.In;

        string sign = IsCredit ? "+" : "-";
        AmountDisplay = $"{sign}{dto.Amount.ToString("N2", CultureInfo.InvariantCulture)}";

        Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "N/A" : dto.Currency;

        Date = dto.CreatedAt.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

        StatusBadgeClass = dto.Status switch
        {
            TransactionStatus.Completed => "bg-success",
            TransactionStatus.Pending   => "bg-warning text-dark",
            TransactionStatus.Failed    => "bg-danger",
            _                           => "bg-secondary"
        };

        StatusDisplay = dto.Status.ToString();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;
}
