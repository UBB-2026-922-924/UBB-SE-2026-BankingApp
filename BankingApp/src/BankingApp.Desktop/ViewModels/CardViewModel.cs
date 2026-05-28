namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Contracts.Features.Cards.Dtos;
using Contracts.Features.Cards.Services;
using BankingApp.Domain.Enums;
using ErrorOr;
using Logging;
using Microsoft.Extensions.Logging;
using Shared;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>Manages card listing and lifecycle actions for the desktop client.</summary>
public partial class CardViewModel : ObservableObject
{
    private readonly ICardService _cardService;
    private readonly ILogger<CardViewModel> _logger;

    private const int CardBrandVisaIndex = 0;
    private const int CardBrandMastercardIndex = 1;
    private const int CardTypeDebitIndex = 0;
    private const int CardTypeCreditIndex = 1;

    /// <summary>Initializes a new instance of the <see cref="CardViewModel"/> class.</summary>
    public CardViewModel(ICardService cardService, ILogger<CardViewModel> logger)
    {
        _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Cards = [];

        FreezeCommand = new AsyncRelayCommand<CardDetailsDto?>(ExecuteFreezeAsync);
        UnfreezeCommand = new AsyncRelayCommand<CardDetailsDto?>(ExecuteUnfreezeAsync);
        CancelCommand = new AsyncRelayCommand<CardDetailsDto?>(ExecuteCancelAsync);
    }

    /// <summary>Gets the command that freezes a card.</summary>
    public IAsyncRelayCommand<CardDetailsDto?> FreezeCommand { get; }

    /// <summary>Gets the command that unfreezes a card.</summary>
    public IAsyncRelayCommand<CardDetailsDto?> UnfreezeCommand { get; }

    /// <summary>Gets the command that cancels a card.</summary>
    public IAsyncRelayCommand<CardDetailsDto?> CancelCommand { get; }

    /// <summary>Gets or sets the currently loaded cards.</summary>
    [ObservableProperty]
    public partial ObservableCollection<CardDetailsDto> Cards { get; set; }

    /// <summary>Gets or sets the currently selected card.</summary>
    [ObservableProperty]
    public partial CardDetailsDto? SelectedCard { get; set; } = null!;

    /// <summary>Gets or sets a human-readable error message to display when an operation fails.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether an error message is currently available.</summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>Gets or sets whether the issue-card form is visible.</summary>
    [ObservableProperty]
    public partial bool IsIssueFormVisible { get; set; } = false;

    /// <summary>Gets or sets the selected card brand index (0 = Visa, 1 = Mastercard).</summary>
    [ObservableProperty]
    public partial int NewCardBrandIndex { get; set; }

    /// <summary>Gets or sets the card type index (0 = Debit, 1 = Credit).</summary>
    [ObservableProperty]
    public partial int NewCardTypeIndex { get; set; }

    /// <summary>Shows the issue-card form and resets its fields.</summary>
    public void ShowIssueForm()
    {
        NewCardBrandIndex = CardBrandVisaIndex;
        NewCardTypeIndex = CardTypeDebitIndex;
        ErrorMessage = string.Empty;
        IsIssueFormVisible = true;
    }

    /// <summary>Hides the issue-card form without submitting.</summary>
    public void HideIssueForm()
    {
        IsIssueFormVisible = false;
    }

    /// <summary>Submits the issue-card form and reloads the card list on success.</summary>
    public async Task<bool> IssueCardAsync()
    {
        string? cardBrand = NewCardBrandIndex switch
        {
            CardBrandVisaIndex => "Visa",
            CardBrandMastercardIndex => "Mastercard",
            _ => null
        };

        CardType cardType = NewCardTypeIndex == CardTypeCreditIndex ? CardType.Credit : CardType.Debit;

        IssueCardRequest request = new()
        {
            CardType = cardType,
            CardBrand = cardBrand
        };

        try
        {
            ErrorOr<CardDetailsDto> result = await _cardService.IssueCardAsync(request);
            if (result.IsError)
            {
                ErrorMessage = "Failed to issue card.";
                return false;
            }

            IsIssueFormVisible = false;
            await LoadAsync();
            return true;
        }
        catch (Exception ex)
        {
            DesktopLogMessages.FailedToIssueCard(_logger, ex);
            ErrorMessage = "An unexpected error occurred while issuing the card.";
            return false;
        }
    }

    /// <summary>Loads the cards from the backend API.</summary>
    public async Task LoadAsync()
    {
        try
        {
            ErrorOr<System.Collections.Generic.List<CardDetailsDto>> result = await _cardService.GetCardsAsync();
            if (result.IsError)
            {
                ErrorMessage = "Failed to load cards.";
                return;
            }

            Cards = new ObservableCollection<CardDetailsDto>(result.Value);
            ErrorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            DesktopLogMessages.FailedToLoadCards(_logger, ex);
            ErrorMessage = "An unexpected error occurred while loading cards.";
        }
    }

    /// <summary>Freezes the specified card.</summary>
    public Task FreezeAsync(CardDetailsDto? card) => ExecuteFreezeAsync(card);

    /// <summary>Unfreezes the specified card.</summary>
    public Task UnfreezeAsync(CardDetailsDto? card) => ExecuteUnfreezeAsync(card);

    /// <summary>Cancels the specified card.</summary>
    public Task CancelAsync(CardDetailsDto? card) => ExecuteCancelAsync(card);

    private async Task ExecuteFreezeAsync(CardDetailsDto? card)
    {
        if (card is null)
        {
            return;
        }

        try
        {
            ErrorOr<Success> result = await _cardService.FreezeCardAsync(card.Id);
            if (result.IsError)
            {
                ErrorMessage = "Failed to freeze card.";
                return;
            }

            await LoadAsync();
        }
        catch (Exception ex)
        {
            DesktopLogMessages.FailedToFreezeCard(_logger, ex, card.Id);
            ErrorMessage = "An unexpected error occurred while freezing the card.";
        }
    }

    private async Task ExecuteUnfreezeAsync(CardDetailsDto? card)
    {
        if (card is null)
        {
            return;
        }

        try
        {
            ErrorOr<Success> result = await _cardService.UnfreezeCardAsync(card.Id);
            if (result.IsError)
            {
                ErrorMessage = "Failed to unfreeze card.";
                return;
            }

            await LoadAsync();
        }
        catch (Exception ex)
        {
            DesktopLogMessages.FailedToUnfreezeCard(_logger, ex, card.Id);
            ErrorMessage = "An unexpected error occurred while unfreezing the card.";
        }
    }

    private async Task ExecuteCancelAsync(CardDetailsDto? card)
    {
        if (card is null)
        {
            return;
        }

        try
        {
            ErrorOr<Success> result = await _cardService.CancelCardAsync(card.Id);
            if (result.IsError)
            {
                ErrorMessage = "Failed to cancel card.";
                return;
            }

            await LoadAsync();
        }
        catch (Exception ex)
        {
            DesktopLogMessages.FailedToCancelCard(_logger, ex, card.Id);
            ErrorMessage = "An unexpected error occurred while cancelling the card.";
        }
    }
}
