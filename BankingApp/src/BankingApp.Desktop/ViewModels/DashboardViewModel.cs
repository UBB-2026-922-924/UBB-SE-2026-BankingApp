namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Contracts.Features.AccountOverview.Dtos;
using Shared.Enums;
using BankingApp.Domain.Enums;
using Contracts.Features.AccountOverview.Services;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Shared;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>Loads and exposes the data needed by the dashboard view.</summary>
public partial class DashboardViewModel : ObservableObject
{
    private const string CardAtStartErrorCode = "dashboard.card_at_start";
    private const string CardAtStartErrorDescription = "Already at the first card.";
    private const string CardAtEndErrorCode = "dashboard.card_at_end";
    private const string CardAtEndErrorDescription = "Already at the last card.";
    private const int FirstCardIndex = 0;
    private const int LastCardIndexOffset = 1;
    private const int CardNumberVisibleSuffixLength = 4;
    private const string FullyMaskedCardNumber = "**** **** **** ****";
    private const string CardNumberMaskPrefix = "**** **** ****";
    private readonly IAccountOverviewService _dashboardService;
    private readonly ILogger<DashboardViewModel> _logger;
    private int _currentCardIndex;

    /// <summary>Initializes a new instance of the <see cref="DashboardViewModel"/> class.</summary>
    public DashboardViewModel(IAccountOverviewService dashboardService, ILogger<DashboardViewModel> logger)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Cards = new List<CardDto>();
        RecentTransactions = new List<TransactionDto>();
        RecentTransactionItems = new List<DashboardTransactionItem>();
        ErrorMessage = string.Empty;
        _currentCardIndex = FirstCardIndex;
    }

    /// <summary>Gets or sets the current dashboard workflow state.</summary>
    [ObservableProperty]
    public partial DashboardState State { get; set; } = DashboardState.Idle;

    /// <summary>Gets the current user summary shown on the dashboard.</summary>
    public UserSummaryDto? CurrentUser { get; private set; }

    /// <summary>Gets the formatted recent-transaction items shown on the dashboard.</summary>
    public List<DashboardTransactionItem> RecentTransactionItems { get; private set; }

    /// <summary>Gets the unread-notification count shown on the dashboard.</summary>
    public int UnreadNotificationCount { get; private set; }

    /// <summary>Gets the latest user-facing dashboard error message.</summary>
    public string ErrorMessage { get; private set; }

    /// <summary>Gets or sets the index of the currently selected payment card.</summary>
    public int CurrentCardIndex
    {
        get => _currentCardIndex;
        private set => _currentCardIndex = Math.Clamp(
            value,
            FirstCardIndex,
            Math.Max(FirstCardIndex, Cards.Count - LastCardIndexOffset));
    }

    /// <summary>Gets a value indicating whether the previous-card action is available.</summary>
    public bool CanNavigatePrevious => Cards.Count > 0 && CurrentCardIndex > FirstCardIndex;

    /// <summary>Gets a value indicating whether the next-card action is available.</summary>
    public bool CanNavigateNext => Cards.Count > 0 && CurrentCardIndex < Cards.Count - LastCardIndexOffset;

    /// <summary>Gets a value indicating whether any cards are available for display.</summary>
    public bool HasCards => Cards.Count > 0;

    /// <summary>Gets the card-page indicator state for the card carousel.</summary>
    public IReadOnlyList<CardPageIndicatorViewModel> CardDots =>
        Cards.Select((_, index) => new CardPageIndicatorViewModel { IsActive = index == CurrentCardIndex }).ToList();

    /// <summary>Gets a value indicating whether recent transactions are available.</summary>
    public bool HasTransactions => RecentTransactionItems.Count > 0;

    /// <summary>Gets the selected card brand display text.</summary>
    public string SelectedCardBrandDisplay =>
        SelectedCard is { } card
            ? string.IsNullOrWhiteSpace(card.CardBrand) ? card.CardType.ToString() : card.CardBrand
            : string.Empty;

    /// <summary>Gets the selected cardholder display text.</summary>
    public string SelectedCardHolderDisplay =>
        SelectedCard is { } card
            ? string.IsNullOrWhiteSpace(card.CardholderName)
                ? "CARD HOLDER"
                : card.CardholderName.ToUpperInvariant()
            : string.Empty;

    /// <summary>Gets the selected card expiry display text.</summary>
    public string SelectedCardExpiryDisplay =>
        SelectedCard?.ExpiryDate.ToString("MM/yy", CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>Gets the masked number of the selected card.</summary>
    public string SelectedCardNumberMasked =>
        SelectedCard is { } card ? MaskCardNumber(card.CardNumber) : FullyMaskedCardNumber;

    private List<CardDto> Cards { get; set; }

    private CardDto? SelectedCard => Cards.Count > 0 ? Cards.ElementAt(CurrentCardIndex) : null;

    private List<TransactionDto> RecentTransactions { get; set; }

    /// <summary>Moves the card carousel to the previous card.</summary>
    public ErrorOr<Success> NavigatePrevious()
    {
        if (!CanNavigatePrevious)
        {
            return Error.Failure(CardAtStartErrorCode, CardAtStartErrorDescription);
        }

        CurrentCardIndex--;
        return Result.Success;
    }

    /// <summary>Moves the card carousel to the next card.</summary>
    public ErrorOr<Success> NavigateNext()
    {
        if (!CanNavigateNext)
        {
            return Error.Failure(CardAtEndErrorCode, CardAtEndErrorDescription);
        }

        CurrentCardIndex++;
        return Result.Success;
    }

    /// <summary>Builds the details string for the currently selected card.</summary>
    public string GetSelectedCardDetails()
    {
        if (SelectedCard is not { } card)
        {
            return string.Empty;
        }

        return
            $"Card Type:       {card.CardType}\n" +
            $"Card Brand:      {card.CardBrand ?? "Mastercard"}\n" +
            $"Card Number:     {MaskCardNumber(card.CardNumber)}\n" +
            $"Cardholder:      {card.CardholderName}\n" +
            $"Expiry Date:     {card.ExpiryDate:MM/yy}\n" +
            $"Status:          {card.Status}\n" +
            $"Contactless:     {(card.IsContactlessEnabled ? "Enabled" : "Disabled")}\n" +
            $"Online Payments: {(card.IsOnlineEnabled ? "Enabled" : "Disabled")}";
    }

    /// <summary>Loads dashboard data for the current user.</summary>
    public async Task<ErrorOr<Success>> LoadDashboard(CancellationToken cancellationToken = default)
    {
        State = DashboardState.Loading;
        ErrorMessage = string.Empty;
        ErrorOr<AccountOverviewDto> result = await _dashboardService.GetDashboardAsync(cancellationToken);
        return result.Match<ErrorOr<Success>>(
            dashboard =>
            {
                if (dashboard.CurrentUser is null)
                {
                    ErrorMessage = UserMessages.Dashboard.IncompleteResponse;
                    State = DashboardState.Error;
                    return Error.Validation(description: UserMessages.Dashboard.IncompleteResponse);
                }

                CurrentUser = dashboard.CurrentUser;
                Cards = dashboard.Cards;
                RecentTransactions = dashboard.RecentTransactions;
                RecentTransactionItems = BuildTransactionItems(RecentTransactions);
                UnreadNotificationCount = dashboard.UnreadNotificationCount;
                _currentCardIndex = FirstCardIndex;
                State = DashboardState.Success;
                return Result.Success;
            },
            errors =>
            {
                ErrorMessage = errors.First().Type switch
                {
                    ErrorType.Unauthorized => UserMessages.Dashboard.SessionExpired,
                    ErrorType.NotFound => UserMessages.Dashboard.NotFound,
                    _ => UserMessages.Dashboard.LoadFailed,
                };
                DesktopLogMessages.LoadDashboardFailed(_logger, errors);
                State = DashboardState.Error;
                return errors.First();
            });
    }

    private static string MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return FullyMaskedCardNumber;
        }

        return cardNumber.Length >= CardNumberVisibleSuffixLength
            ? $"{CardNumberMaskPrefix} {cardNumber[^CardNumberVisibleSuffixLength..]}"
            : FullyMaskedCardNumber;
    }

    private static List<DashboardTransactionItem> BuildTransactionItems(IEnumerable<TransactionDto> transactions)
    {
        return transactions
            .Select(transaction => new DashboardTransactionItem
            {
                MerchantDisplayName = GetMerchantDisplayName(transaction),
                Currency = GetValueOrFallback(transaction.Currency, "N/A"),
                AmountDisplay = FormatAmountDisplay(transaction),
            })
            .ToList();
    }

    private static string GetMerchantDisplayName(TransactionDto transaction)
    {
        return FirstNonEmpty(
            transaction.MerchantName,
            transaction.Description,
            transaction.CounterpartyName,
            "Transaction");
    }

    private static string FormatAmountDisplay(TransactionDto transaction)
    {
        string sign = transaction.Direction switch
        {
            TransactionDirection.Out => "-",
            TransactionDirection.In => "+",
            _ => throw new ArgumentOutOfRangeException(
                nameof(transaction),
                transaction.Direction,
                "Unsupported transaction direction."),
        };
        return $"{sign}{transaction.Amount.ToString("N2", CultureInfo.InvariantCulture)}";
    }

    private static string GetValueOrFallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
}
