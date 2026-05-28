namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Contracts.Features.Transfers.Dtos;
using Contracts.Features.Transfers.Services;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Shared;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>
///     Provides data for the transfer history page.
///     Loads the authenticated user's past transfers from the API and exposes them
///     as pre-formatted <see cref="TransferHistoryDisplayItem"/> rows ready for binding.
/// </summary>
public partial class TransferHistoryViewModel : ObservableObject
{
    private const string DateTimeFormat = "dd MMM yyyy, HH:mm";
    private const string FallbackReference = "—";

    private readonly ILogger<TransferHistoryViewModel> _logger;
    private readonly ITransferService _transferService;

    /// <summary>Initializes a new instance of the <see cref="TransferHistoryViewModel"/> class.</summary>
    public TransferHistoryViewModel(
        ITransferService transferService,
        ILogger<TransferHistoryViewModel> logger)
    {
        _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Transfers = [];
    }

    /// <summary>Gets the pre-formatted transfer items shown in the list.</summary>
    public ObservableCollection<TransferHistoryDisplayItem> Transfers { get; }

    /// <summary>Gets or sets the error message shown when loading fails.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(HasNoTransfers))]
    [NotifyPropertyChangedFor(nameof(ShowTransferList))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether an error message is currently active.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>Gets or sets a value indicating whether data is currently being fetched.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoTransfers))]
    [NotifyPropertyChangedFor(nameof(ShowTransferList))]
    public partial bool IsLoading { get; set; } = false;

    /// <summary>Gets a value indicating whether the list is empty and no error occurred.</summary>
    public bool HasNoTransfers => !IsLoading && !HasError && Transfers.Count == 0;

    /// <summary>Gets a value indicating whether the transfer list should be visible.</summary>
    public bool ShowTransferList => !IsLoading && !HasError && Transfers.Count > 0;

    /// <summary>Fetches the authenticated user's transfer history from the server.</summary>
    public async Task LoadHistoryAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        Transfers.Clear();

        try
        {
            ErrorOr<List<TransferResponse>> result = await _transferService.GetHistoryAsync();

            if (result.IsError)
            {
                ErrorMessage = UserMessages.TransferHistory.LoadFailed;
                return;
            }

            foreach (TransferResponse transfer in result.Value)
            {
                Transfers.Add(MapToDisplayItem(transfer));
            }

            OnPropertyChanged(nameof(HasNoTransfers));
            OnPropertyChanged(nameof(ShowTransferList));
        }
        catch (Exception loadException)
        {
            DesktopLogMessages.LoadTransferHistoryFailedUnexpected(_logger, loadException);
            ErrorMessage = UserMessages.TransferHistory.LoadFailed;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static TransferHistoryDisplayItem MapToDisplayItem(TransferResponse transfer)
    {
        string amountDisplay =
            $"-{transfer.Amount.ToString("F2", CultureInfo.InvariantCulture)}";
        string dateDisplay =
            transfer.CreatedAt.ToLocalTime().ToString(DateTimeFormat, CultureInfo.CurrentCulture);
        string referenceDisplay = string.IsNullOrWhiteSpace(transfer.Reference)
            ? FallbackReference
            : transfer.Reference;

        return new TransferHistoryDisplayItem
        {
            RecipientName = transfer.RecipientName,
            RecipientIban = transfer.RecipientIban,
            BankName = transfer.RecipientBankName ?? string.Empty,
            Amount = amountDisplay,
            Currency = transfer.Currency,
            DateDisplay = dateDisplay,
            StatusDisplay = transfer.Status.ToString(),
            ReferenceDisplay = referenceDisplay,
        };
    }
}
