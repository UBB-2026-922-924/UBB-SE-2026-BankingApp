namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.Billers.Dtos;
using ErrorOr;

public partial class BillPayViewModel
{
    /// <summary>Loads billers, saved billers, and source accounts for the workflow.</summary>
    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            ResetFormStateOnly();

            await LoadBillersAsync();
            await LoadSavedBillersAsync();
            await LoadAccountsAsync();
        }
        catch (Exception loadException)
        {
            ErrorMessage = $"Failed to load data: {loadException.Message}";
        }
    }

    internal void ExecuteSearch()
    {
        _ = SearchBillersAsync();
    }

    internal void ExecuteSelectBiller(object? parameter)
    {
        ErrorMessage = string.Empty;

        switch (parameter)
        {
            case BillerDto biller:
                SelectedBiller = biller;
                CurrentStep = PaymentDetailsStep;
                return;
            case SavedBillerDto savedBiller:
                SelectSavedBiller(savedBiller);
                return;
        }
    }

    internal async Task ExecuteNextStepAsync()
    {
        ErrorMessage = string.Empty;

        switch (CurrentStep)
        {
            case SelectBillerStep:
                MoveFromBillerSelection();
                break;
            case PaymentDetailsStep:
                await MoveFromPaymentDetailsAsync();
                break;
        }
    }

    internal void ExecuteBack()
    {
        ErrorMessage = string.Empty;

        if (CurrentStep <= SelectBillerStep)
        {
            return;
        }

        CurrentStep = CurrentStep switch
        {
            ReviewAndConfirmStep => PaymentDetailsStep,
            _ => CurrentStep - 1,
        };
    }

    internal async Task ExecutePayBillAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (!ValidatePaymentRequest())
            {
                return;
            }

            BillPayRequest request = BuildBillPaymentRequest();
            ErrorOr<BillPayResponse> payResult = await _billPaymentService.PayBillAsync(request);

            if (payResult.IsError)
            {
                ErrorMessage = $"Payment failed: {payResult.FirstError.Description}";
                return;
            }

            await SaveSelectedBillerIfNeededAsync();
            ApplyPaymentSuccess(payResult.Value);
        }
        catch (Exception paymentException)
        {
            ErrorMessage = $"Payment failed: {paymentException.Message}";
        }
    }

    internal void ResetForm()
    {
        ErrorMessage = string.Empty;
        ResetFormStateOnly();

        // Refresh the biller list so the cleared search/category filters take effect.
        ExecuteSearch();
    }

    internal async Task ExecuteRemoveSavedBillerAsync(SavedBillerDto? savedBiller)
    {
        if (savedBiller == null)
        {
            return;
        }

        ErrorMessage = string.Empty;

        ErrorOr<Success> result = await _billerService.DeleteSavedBillerAsync(savedBiller.Id);
        if (result.IsError)
        {
            ErrorMessage = $"Failed to remove saved biller: {result.FirstError.Description}";
            return;
        }

        SavedBillerDto? existing = SavedBillers.FirstOrDefault(saved => saved.Id == savedBiller.Id);
        if (existing != null)
        {
            SavedBillers.Remove(existing);
        }
    }

    private async Task LoadBillersAsync()
    {
        ErrorOr<List<BillerDto>> result = await _billerService.GetBillersAsync();
        if (!result.IsError)
        {
            Billers = new ObservableCollection<BillerDto>(result.Value);
        }
    }

    private async Task LoadSavedBillersAsync()
    {
        ErrorOr<List<SavedBillerDto>> result = await _billerService.GetSavedBillersAsync();
        if (!result.IsError)
        {
            SavedBillers = new ObservableCollection<SavedBillerDto>(result.Value);
        }
    }

    private async Task LoadAccountsAsync()
    {
        ErrorOr<List<AccountDto>> result = await _billPaymentService.GetAccountsAsync();
        if (!result.IsError)
        {
            Accounts = new ObservableCollection<AccountDto>(result.Value);
        }
    }

    internal async Task SearchBillersAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            // "All categories" is a sentinel meaning no category filter.
            string? category = string.Equals(SelectedCategory, AllCategoriesOption, StringComparison.OrdinalIgnoreCase)
                ? null
                : SelectedCategory;

            ErrorOr<List<BillerDto>> result = await _billerService.GetBillersAsync(SearchQuery, category);

            if (!result.IsError && result.Value is not null)
            {
                Billers = new ObservableCollection<BillerDto>(result.Value);
            }
        }
        catch (Exception searchException)
        {
            ErrorMessage = $"Search failed: {searchException.Message}";
        }
    }

    private void SelectSavedBiller(SavedBillerDto savedBiller)
    {
        SelectedBiller = savedBiller.ToBiller();
        if (!string.IsNullOrWhiteSpace(savedBiller.DefaultReference))
        {
            BillerReference = savedBiller.DefaultReference!;
        }

        CurrentStep = PaymentDetailsStep;
    }

    private void MoveFromBillerSelection()
    {
        if (SelectedBiller == null)
        {
            ErrorMessage = "Please select a biller.";
            return;
        }

        CurrentStep = PaymentDetailsStep;
    }

    private async Task MoveFromPaymentDetailsAsync()
    {
        if (!ValidatePaymentRequest())
        {
            return;
        }

        await SetFeeAsync();
        CurrentStep = ReviewAndConfirmStep;
    }

    private bool ValidatePaymentRequest()
    {
        if (SelectedBiller == null)
        {
            ErrorMessage = "Please select a biller.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(BillerReference))
        {
            ErrorMessage = "Please enter a biller reference.";
            return false;
        }

        if (SelectedAccount == null)
        {
            ErrorMessage = "Please select a source account.";
            return false;
        }

        if (Amount <= MinimumAmount)
        {
            ErrorMessage = "Please enter a valid amount.";
            return false;
        }

        return true;
    }

    private async Task SetFeeAsync()
    {
        ErrorOr<FeeResponse> feeResult = await _billPaymentService.GetFeeAsync(Amount);
        Fee = !feeResult.IsError ? feeResult.Value.Fee : NoFee;
    }

    private BillPayRequest BuildBillPaymentRequest() =>
        new()
        {
            SourceAccountId = SelectedAccount!.Id,
            BillerId = SelectedBiller!.Id,
            BillerReference = BillerReference,
            Amount = Amount,
            IsPayInFull = false,
        };

    private async Task SaveSelectedBillerIfNeededAsync()
    {
        if (!ShouldSaveBiller || SelectedBiller == null)
        {
            return;
        }

        bool alreadySavedWithSameReference = SavedBillers.Any(savedBiller =>
            savedBiller.BillerId == SelectedBiller.Id &&
            string.Equals(savedBiller.DefaultReference, BillerReference, StringComparison.OrdinalIgnoreCase));

        if (alreadySavedWithSameReference)
        {
            return;
        }

        SaveBillerRequest saveRequest = new()
        {
            BillerId = SelectedBiller.Id,
            Nickname = SelectedBiller.Name,
            DefaultReference = BillerReference,
        };

        ErrorOr<SavedBillerDto> saveResult = await _billerService.SaveBillerAsync(saveRequest);
        if (saveResult.IsError)
        {
            return;
        }

        // The save endpoint upserts, so replace any existing entry for this biller instead of
        // appending a duplicate when only the reference changed.
        SavedBillerDto? existing = SavedBillers.FirstOrDefault(savedBiller => savedBiller.BillerId == saveResult.Value.BillerId);
        if (existing != null)
        {
            int index = SavedBillers.IndexOf(existing);
            SavedBillers[index] = saveResult.Value;
        }
        else
        {
            SavedBillers.Add(saveResult.Value);
        }
    }

    private void ApplyPaymentSuccess(BillPayResponse response)
    {
        ReceiptNumber = response.ReceiptNumber;
        Fee = response.Fee;
        CurrentStep = PaymentResultStep;
    }

    private void ResetFormStateOnly()
    {
        CurrentStep = SelectBillerStep;
        SelectedBiller = null;
        SearchQuery = string.Empty;
        SelectedCategory = null;
        BillerReference = string.Empty;
        Amount = MinimumAmount;
        Fee = NoFee;
        ReceiptNumber = string.Empty;
        SelectedAccount = null;
        IsPayInFull = false;
        ShouldSaveBiller = false;
    }

    private void ApplySavedDefaultsForSelectedBiller()
    {
        if (SelectedBiller == null)
        {
            ShouldSaveBiller = false;
            return;
        }

        SavedBillerDto? matchingSaved = SavedBillers.FirstOrDefault(s => s.BillerId == SelectedBiller.Id);

        // Reflect whether this biller is already saved so the toggle starts in the right state.
        ShouldSaveBiller = matchingSaved != null;

        if (matchingSaved != null &&
            string.IsNullOrWhiteSpace(BillerReference) &&
            !string.IsNullOrWhiteSpace(matchingSaved.DefaultReference))
        {
            BillerReference = matchingSaved.DefaultReference!;
        }
    }
}
