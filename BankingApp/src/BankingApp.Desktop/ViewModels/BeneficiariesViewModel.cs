namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Features.Beneficiaries.Services;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Navigation;
using State;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>
///     View model for the beneficiaries page in the desktop application.
/// </summary>
public partial class BeneficiariesViewModel : ObservableObject
{
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly ILogger<BeneficiariesViewModel> _logger;
    private readonly IAppNavigationService _navigationService;
    private readonly ITransferDraftState _transferDraftState;

    /// <summary>Initializes a new instance of the <see cref="BeneficiariesViewModel"/> class.</summary>
    public BeneficiariesViewModel(
        IBeneficiaryService beneficiaryService,
        IAppNavigationService navigationService,
        ITransferDraftState transferDraftState,
        ILogger<BeneficiariesViewModel> logger)
    {
        _beneficiaryService = beneficiaryService ?? throw new ArgumentNullException(nameof(beneficiaryService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _transferDraftState = transferDraftState ?? throw new ArgumentNullException(nameof(transferDraftState));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Beneficiaries = [];
    }

    /// <summary>Gets the currently loaded list of beneficiaries.</summary>
    public ObservableCollection<BeneficiaryDto> Beneficiaries { get; }

    /// <summary>Gets a value indicating whether any beneficiaries are available.</summary>
    public bool HasBeneficiaries => Beneficiaries.Count > 0;

    /// <summary>Gets a value indicating whether an error is currently shown.</summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>Gets the current form title.</summary>
    public string FormTitle => IsEditMode ? "Edit beneficiary" : "Add beneficiary";

    /// <summary>Gets the current save button label.</summary>
    public string SaveButtonText => IsEditMode ? "Save changes" : "Save beneficiary";

    /// <summary>Gets or sets a value indicating whether the beneficiary form is visible.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormTitle))]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    public partial bool IsFormVisible { get; set; }

    /// <summary>Gets or sets a value indicating whether the form edits an existing beneficiary.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormTitle))]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    public partial bool IsEditMode { get; set; }

    /// <summary>Gets or sets the id of the beneficiary being edited.</summary>
    [ObservableProperty]
    public partial int EditingBeneficiaryId { get; set; }

    /// <summary>Gets or sets the beneficiary name currently entered in the form.</summary>
    [ObservableProperty]
    public partial string BeneficiaryName { get; set; } = string.Empty;

    /// <summary>Gets or sets the beneficiary IBAN currently entered in the form.</summary>
    [ObservableProperty]
    public partial string BeneficiaryIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the beneficiary bank name currently entered in the form.</summary>
    [ObservableProperty]
    public partial string BeneficiaryBankName { get; set; } = string.Empty;

    /// <summary>Gets or sets the current error message.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Loads the beneficiaries from the backend API.</summary>
    public async Task<ErrorOr<Success>> LoadBeneficiariesAsync()
    {
        try
        {
            ErrorOr<List<BeneficiaryDto>> result = await _beneficiaryService.GetAllAsync();
            if (result.IsError)
            {
                ErrorMessage = "Failed to load beneficiaries.";
                return result.FirstError;
            }

            ReplaceBeneficiaries(result.Value);
            ErrorMessage = string.Empty;
            return Result.Success;
        }
        catch (Exception loadBeneficiariesException)
        {
            DesktopLogMessages.FailedToLoadBeneficiaries(_logger, loadBeneficiariesException);
            ErrorMessage = "An unexpected error occurred while loading beneficiaries.";
            return Error.Failure();
        }
    }

    /// <summary>Begins the add beneficiary flow.</summary>
    public void StartAddBeneficiary()
    {
        IsEditMode = false;
        EditingBeneficiaryId = 0;
        BeneficiaryName = string.Empty;
        BeneficiaryIban = string.Empty;
        BeneficiaryBankName = string.Empty;
        ErrorMessage = string.Empty;
        IsFormVisible = true;
    }

    /// <summary>Begins the edit beneficiary flow.</summary>
    public void StartEditBeneficiary(BeneficiaryDto beneficiary)
    {
        ArgumentNullException.ThrowIfNull(beneficiary);

        IsEditMode = true;
        EditingBeneficiaryId = beneficiary.Id;
        BeneficiaryName = beneficiary.Name ?? string.Empty;
        BeneficiaryIban = beneficiary.Iban ?? string.Empty;
        BeneficiaryBankName = beneficiary.BankName ?? string.Empty;
        ErrorMessage = string.Empty;
        IsFormVisible = true;
    }

    /// <summary>Cancels the current add or edit operation.</summary>
    public void CancelEditing()
    {
        IsFormVisible = false;
        IsEditMode = false;
        EditingBeneficiaryId = 0;
        BeneficiaryName = string.Empty;
        BeneficiaryIban = string.Empty;
        BeneficiaryBankName = string.Empty;
    }

    /// <summary>Saves the beneficiary currently entered in the form.</summary>
    public async Task<bool> SaveBeneficiaryAsync()
    {
        try
        {
            string trimmedName = BeneficiaryName.Trim();
            string trimmedIban = BeneficiaryIban.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
            string? trimmedBankName = string.IsNullOrWhiteSpace(BeneficiaryBankName)
                ? null
                : BeneficiaryBankName.Trim();

            ErrorOr<Success> result = IsEditMode
                ? await _beneficiaryService.UpdateAsync(
                    EditingBeneficiaryId,
                    new UpdateBeneficiaryRequest
                    {
                        Id = EditingBeneficiaryId,
                        Name = trimmedName,
                        Iban = trimmedIban,
                        BankName = trimmedBankName,
                    })
                : await _beneficiaryService.CreateAsync(
                    new CreateBeneficiaryRequest
                    {
                        Name = trimmedName,
                        Iban = trimmedIban,
                        BankName = trimmedBankName,
                    });

            if (result.IsError)
            {
                ErrorMessage = IsEditMode
                    ? "Failed to update beneficiary."
                    : "Failed to save beneficiary.";
                return false;
            }

            await LoadBeneficiariesAsync();
            CancelEditing();
            return true;
        }
        catch (Exception saveBeneficiaryException)
        {
            DesktopLogMessages.FailedToAddBeneficiary(_logger, saveBeneficiaryException);
            ErrorMessage = IsEditMode
                ? "An unexpected error occurred while updating the beneficiary."
                : "An unexpected error occurred while saving the beneficiary.";
            return false;
        }
    }

    /// <summary>Deletes the beneficiary with the specified id.</summary>
    public async Task<bool> DeleteBeneficiaryAsync(int beneficiaryId)
    {
        try
        {
            ErrorOr<Success> result = await _beneficiaryService.DeleteAsync(beneficiaryId);
            if (result.IsError)
            {
                ErrorMessage = "Failed to delete beneficiary.";
                return false;
            }

            BeneficiaryDto? beneficiary = Beneficiaries.FirstOrDefault(currentBeneficiary => currentBeneficiary.Id == beneficiaryId);
            if (beneficiary is not null)
            {
                Beneficiaries.Remove(beneficiary);
                OnPropertyChanged(nameof(HasBeneficiaries));
            }

            ErrorMessage = string.Empty;
            return true;
        }
        catch (Exception deleteBeneficiaryException)
        {
            DesktopLogMessages.FailedToDeleteBeneficiary(_logger, deleteBeneficiaryException, beneficiaryId);
            ErrorMessage = "An unexpected error occurred while deleting the beneficiary.";
            return false;
        }
    }

    /// <summary>Uses the selected beneficiary as the next transfer recipient.</summary>
    public void UseForTransfer(BeneficiaryDto beneficiary)
    {
        ArgumentNullException.ThrowIfNull(beneficiary);

        _transferDraftState.SetDraft(
            beneficiary.Name ?? string.Empty,
            beneficiary.Iban ?? string.Empty);
        _navigationService.NavigateToContent<Views.TransferView>();
    }

    private void ReplaceBeneficiaries(IEnumerable<BeneficiaryDto> beneficiaries)
    {
        Beneficiaries.Clear();

        foreach (BeneficiaryDto beneficiary in beneficiaries.OrderBy(currentBeneficiary => currentBeneficiary.Name))
        {
            Beneficiaries.Add(beneficiary);
        }

        OnPropertyChanged(nameof(HasBeneficiaries));
    }
}
