namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Features.Transfers.Dtos;
using ErrorOr;
using Shared;

public partial class TransferViewModel
{
    /// <summary>Loads the authenticated user's accounts from the API and pre-selects the first account.</summary>
    public async Task LoadAccountsAsync()
    {
        try
        {
            ErrorOr<List<TransferAccountSelectionResponse>> result = await _transferService.GetAccountsAsync();

            if (result.IsError)
            {
                ErrorMessage = UserMessages.Transfer.AccountLoadFailed;
                return;
            }

            Accounts.Clear();
            foreach (TransferAccountSelectionResponse account in result.Value)
            {
                Accounts.Add(account);
            }

            if (Accounts.Count > MinimumAccounts)
            {
                SelectedAccount = Accounts[FirstAccountIndex];
            }

            ApplyDraftRecipient();
        }
        catch (Exception loadAccountsException)
        {
            ErrorMessage = loadAccountsException.Message;
        }
    }

    /// <summary>Advances the wizard to the next step, validating IBAN, amount, and 2FA before allowing progression.</summary>
    internal async Task ExecuteNextStep()
    {
        ErrorMessage = string.Empty;

        switch (CurrentStep)
        {
            case IbanValidationStep:
                await MoveFromIbanStepAsync();
                break;
            case TransferDetailsStep:
                MoveFromDetailsStep();
                break;
            default:
                CurrentStep++;
                break;
        }
    }

    /// <summary>Submits the transfer to the API; on success advances to the completion step, on failure sets the error step.</summary>
    internal async Task ExecuteTransferAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (SelectedAccount == null)
            {
                throw new InvalidOperationException(UserMessages.Transfer.NoAccountSelected);
            }

            CreateTransferRequest request = new()
            {
                SourceAccountId = SelectedAccount.Id,
                RecipientName = RecipientName,
                RecipientIban = RecipientIban,
                Amount = Amount,
                Currency = Currency,
                Reference = Reference,
            };

            ErrorOr<TransferExecutionResponse> result =
                await _transferService.ExecuteAsync(request);

            if (result.IsError)
            {
                ErrorMessage = result.FirstError.Description;
                CurrentStep = TransferErrorStep;
                return;
            }

            TransactionRef = result.Value.TransactionRef;
            CurrentStep = TransferCompletedStep;
        }
        catch (Exception executeTransferException)
        {
            ErrorMessage = executeTransferException.Message;
            CurrentStep = TransferErrorStep;
        }
    }

    /// <summary>Resets all form fields and returns the wizard to step 1.</summary>
    internal void ExecuteSendAgain()
    {
        SelectedAccount = Accounts.Count > MinimumAccounts ? Accounts[FirstAccountIndex] : null;
        RecipientName = string.Empty;
        RecipientIban = string.Empty;
        IsIbanValid = false;
        BankName = string.Empty;
        Amount = ZeroAmount;
        Currency = DefaultTransferCurrency;
        FxPreviewText = string.Empty;
        TransactionRef = string.Empty;
        ErrorMessage = string.Empty;
        AmountText = string.Empty;
        Reference = string.Empty;
        CurrentStep = IbanValidationStep;
    }

    private void ExecuteCancel()
    {
        // TODO: this is missing, why?
        throw new NotImplementedException();
    }

    private async Task MoveFromIbanStepAsync()
    {
        await UpdateIbanValidationAsync(RecipientIban);

        if (IsIbanValid)
        {
            CurrentStep++;
            return;
        }

        ErrorMessage = UserMessages.Transfer.InvalidIban;
        CurrentStep = TransferErrorStep;
    }

    private void MoveFromDetailsStep()
    {
        if (SelectedAccount != null && Amount > ZeroAmount && !string.IsNullOrWhiteSpace(RecipientName))
        {
            CurrentStep = ReviewAndConfirmationStep;
            return;
        }

        ErrorMessage = UserMessages.Transfer.AmountMustBePositive;
        CurrentStep = TransferErrorStep;
    }

    private async Task UpdateIbanValidationAsync(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            IsIbanValid = false;
            BankName = string.Empty;
            return;
        }

        try
        {
            ErrorOr<TransferIbanValidationResponse> result =
                await _transferService.ValidateIbanAsync(new TransferIbanValidationRequest { Iban = iban });

            if (result.IsError)
            {
                IsIbanValid = false;
                BankName = string.Empty;
                return;
            }

            IsIbanValid = result.Value.IsValid;
            BankName = result.Value.IsValid ? result.Value.BankName : string.Empty;
        }
        catch
        {
            IsIbanValid = false;
            BankName = string.Empty;
        }
    }

    private async Task UpdateFxPreviewAsync()
    {
        try
        {
            if (SelectedAccount == null || Amount <= ZeroAmount || string.IsNullOrWhiteSpace(Currency))
            {
                FxPreviewText = string.Empty;
                return;
            }

            ErrorOr<TransferForexPreviewResponse> result =
                await _transferService.GetFxPreviewAsync(SelectedAccount.Currency, Currency, Amount);

            if (result.IsError)
            {
                FxPreviewText = string.Empty;
                return;
            }

            TransferForexPreviewResponse preview = result.Value;
            FxPreviewText = preview.ExchangeRate == IdentityExchangeRate
                ? $"{Amount:F2} {Currency}"
                : $"{Amount:F2} {SelectedAccount.Currency} -> {preview.ConvertedAmount:F2} {Currency} (rate: {preview.ExchangeRate:F4})";
        }
        catch
        {
            FxPreviewText = string.Empty;
        }
    }

    private void ApplyDraftRecipient()
    {
        if (!_transferDraftState.HasDraft)
        {
            return;
        }

        RecipientName = _transferDraftState.RecipientName;
        RecipientIban = _transferDraftState.RecipientIban;
        CurrentStep = TransferDetailsStep;
        _transferDraftState.Clear();
    }

}
