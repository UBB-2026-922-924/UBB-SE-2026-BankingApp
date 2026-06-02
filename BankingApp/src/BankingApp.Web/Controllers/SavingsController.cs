namespace BankingApp.Web.Controllers;

using System.Globalization;
using Contracts.Features.Investments;
using Contracts.Features.Savings.Dtos;
using BankingApp.Contracts.Http;
using Domain.Aggregates.SavingsAggregate;
using Domain.Aggregates.SavingsAggregate.Entities;
using Domain.Enums;
using BankingApp.Infrastructure.Http.Features.Savings.Services;
using Models.Savings;
using Microsoft.AspNetCore.Mvc;

public class SavingsController(
    ISavingsRepoProxy savingsRepoProxy,
    ISavingsUiRulesRepoProxy savingsUiRulesRepoProxy,
    ISavingsWorkflowRepoProxy savingsWorkflowRepoProxy,
    ISavingsPresentationRepoProxy savingsPresentationRepoProxy) : Controller
{
    private const string OverviewTab = "overview";
    private const string DepositTab = "deposit";
    private const string WithdrawTab = "withdraw";
    private const string AutoDepositTab = "auto";
    private const string CloseTab = "close";

    [HttpGet]
    public async Task<IActionResult> Index(int? accountId = null, string manageTab = OverviewTab)
    {
        try
        {
            SavingsPageViewModel model = await BuildPageModelAsync(accountId, manageTab);
            return View(model);
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
            return View(new SavingsPageViewModel { ActiveManageTab = manageTab });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount([Bind(Prefix = "CreateAccount")] SavingsCreateAccountFormModel createAccount)
    {
        try
        {
            List<FundingSourceOption> fundingSources = await savingsRepoProxy.GetFundingSourcesAsync(CurrentUserId);
            NormalizeCreateAccountForm(createAccount, fundingSources);
            ModelState.Clear();

            decimal? parsedTargetAmount = null;
            if (IsGoalSavings(createAccount.SelectedSavingsType) && !string.IsNullOrWhiteSpace(createAccount.TargetAmount))
            {
                parsedTargetAmount = await savingsUiRulesRepoProxy.ParsePositiveAmount(createAccount.TargetAmount);
            }

            Dictionary<string, string> validation = await savingsUiRulesRepoProxy.ValidateCreateAccount(new ValidateCreateAccountRequest
            {
                SelectedSavingsType = createAccount.SelectedSavingsType,
                AccountName = createAccount.AccountName,
                InitialDepositText = createAccount.InitialDeposit,
                HasFundingSource = createAccount.FundingSourceId.HasValue,
                SelectedFrequency = createAccount.SelectedFrequency,
                TargetAmount = parsedTargetAmount,
                TargetDate = createAccount.TargetDate.HasValue ? new DateTimeOffset(createAccount.TargetDate.Value) : null,
                IsGoalSavings = IsGoalSavings(createAccount.SelectedSavingsType),
            });

            AddCreateAccountErrors(validation);

            if (validation.Count > 0)
            {
                TempData["Error"] = "Please correct the highlighted fields.";
                SavingsPageViewModel invalidModel = await BuildPageModelAsync(null, OverviewTab, createAccount);
                return View("Index", invalidModel);
            }

            decimal initialDeposit = await savingsUiRulesRepoProxy.ParsePositiveAmount(createAccount.InitialDeposit);
            DepositFrequency? depositFrequency = string.Equals(createAccount.SelectedFrequency, "OneTime", StringComparison.OrdinalIgnoreCase)
                                                 || string.IsNullOrWhiteSpace(createAccount.SelectedFrequency)
                ? null
                : await savingsUiRulesRepoProxy.ParseDepositFrequency(createAccount.SelectedFrequency);

            await savingsRepoProxy.CreateSavingsAccountAsync(new CreateSavingsAccountDto
            {
                UserIdentificationNumber = CurrentUserId,
                SavingsType = createAccount.SelectedSavingsType,
                AccountName = createAccount.AccountName.Trim(),
                InitialDeposit = initialDeposit,
                FundingAccountId = createAccount.FundingSourceId!.Value,
                TargetAmount = IsGoalSavings(createAccount.SelectedSavingsType) ? parsedTargetAmount : null,
                TargetDate = IsGoalSavings(createAccount.SelectedSavingsType) ? createAccount.TargetDate : null,
                MaturityDate = IsFixedDeposit(createAccount.SelectedSavingsType) ? createAccount.MaturityDate : null,
                DepositFrequency = depositFrequency,
            }, apy: 0m);

            TempData["Success"] = "Savings account opened successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
            SavingsPageViewModel invalidModel = await BuildPageModelAsync(null, OverviewTab, createAccount);
            return View("Index", invalidModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit([Bind(Prefix = "Deposit")] SavingsDepositFormModel deposit)
    {
        try
        {
            decimal amount = await savingsUiRulesRepoProxy.ParsePositiveAmount(deposit.Amount);
            List<FundingSourceOption> fundingSources = await savingsRepoProxy.GetFundingSourcesAsync(CurrentUserId);
            FundingSourceOption? selectedFundingSource = fundingSources.FirstOrDefault(source => source.Id == deposit.FundingSourceId);
            if (selectedFundingSource == null)
            {
                TempData["Error"] = "Select a funding source before submitting the deposit.";
                return RedirectToAction(nameof(Index), new { accountId = deposit.AccountId, manageTab = DepositTab });
            }

            DepositResponseDto response = await savingsRepoProxy.DepositAsync(deposit.AccountId, amount, selectedFundingSource.DisplayName);
            TempData["Success"] = $"Deposit successful. New balance: {response.NewBalance:C2}.";
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { accountId = deposit.AccountId, manageTab = DepositTab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw([Bind(Prefix = "Withdraw")] SavingsWithdrawFormModel withdraw)
    {
        try
        {
            decimal amount = await savingsUiRulesRepoProxy.ParsePositiveAmount(withdraw.Amount);
            List<FundingSourceOption> fundingSources = await savingsRepoProxy.GetFundingSourcesAsync(CurrentUserId);
            FundingSourceOption? destination = fundingSources.FirstOrDefault(source => source.Id == withdraw.DestinationId);
            ValidationResponse validation = await savingsWorkflowRepoProxy.ValidateWithdrawRequest(amount, destination);
            if (!validation.IsValid)
            {
                TempData["Error"] = validation.ErrorMessage;
                return RedirectToAction(nameof(Index), new { accountId = withdraw.AccountId, manageTab = WithdrawTab });
            }

            WithdrawResponseDto response = await savingsRepoProxy.WithdrawAsync(withdraw.AccountId, amount, destination!.DisplayName, earlyWithdrawalPenalty: 0m);
            string resultMessage = await savingsWorkflowRepoProxy.BuildWithdrawResultMessage(response);
            TempData[response.Success ? "Success" : "Error"] = resultMessage;
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { accountId = withdraw.AccountId, manageTab = WithdrawTab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAutoDeposit([Bind(Prefix = "AutoDeposit")] SavingsAutoDepositFormModel autoDeposit)
    {
        try
        {
            decimal amount = await savingsUiRulesRepoProxy.ParsePositiveAmount(autoDeposit.Amount);
            DepositFrequency frequency = await savingsUiRulesRepoProxy.ParseDepositFrequency(autoDeposit.Frequency);
            AutoDeposit? existingAutoDeposit = await savingsRepoProxy.GetAutoDepositAsync(autoDeposit.AccountId);
            await savingsRepoProxy.SaveAutoDepositAsync(AutoDeposit.Reconstitute(
                existingAutoDeposit?.Id ?? 0,
                autoDeposit.AccountId,
                amount,
                frequency,
                autoDeposit.StartDate ?? DateTime.Today.AddDays(1),
                autoDeposit.IsActive,
                sourceAccountId: null,
                dayOfMonth: null,
                dayOfWeek: null,
                updatedAt: null));

            TempData["Success"] = "Auto deposit saved successfully.";
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { accountId = autoDeposit.AccountId, manageTab = AutoDepositTab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseAccount([Bind(Prefix = "CloseAccount")] SavingsCloseAccountFormModel closeAccount)
    {
        try
        {
            ValidationResponse validation = await savingsWorkflowRepoProxy.ValidateCloseConfirmation(closeAccount.Confirmed, closeAccount.DestinationAccountId);
            if (!validation.IsValid)
            {
                TempData["Error"] = validation.ErrorMessage;
                return RedirectToAction(nameof(Index), new { accountId = closeAccount.AccountId, manageTab = CloseTab });
            }

            ClosureResultDto response = await savingsRepoProxy.CloseSavingsAccountAsync(
                closeAccount.AccountId,
                closeAccount.DestinationAccountId,
                transferAmount: 0m,
                earlyClosurePenalty: 0m);

            TempData[response.Success ? "Success" : "Error"] =
                response.Success
                    ? $"Account closed successfully. Transferred {response.TransferredAmount:C2}."
                    : response.Message;
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { accountId = closeAccount.AccountId, manageTab = CloseTab });
    }

    [HttpGet]
    public async Task<IActionResult> DepositPreview(int accountId, string amountText)
    {
        try
        {
            SavingsAccount? selectedAccount = (await savingsRepoProxy.GetSavingsAccountsByUserIdAsync(CurrentUserId, true))
                .FirstOrDefault(account => account.Id == accountId);
            if (selectedAccount == null)
            {
                return Json(new { preview = string.Empty });
            }

            string preview = await savingsUiRulesRepoProxy.GetDepositPreview(amountText, selectedAccount);
            return Json(new { preview });
        }
        catch (Exception)
        {
            return Json(new { preview = string.Empty });
        }
    }

    [HttpGet]
    public async Task<IActionResult> WithdrawPreview(int accountId, string amountText)
    {
        try
        {
            SavingsAccount? selectedAccount = (await savingsRepoProxy.GetSavingsAccountsByUserIdAsync(CurrentUserId, true))
                .FirstOrDefault(account => account.Id == accountId);
            if (selectedAccount == null)
            {
                return Json(new SavingsWithdrawPreviewViewModel());
            }

            SavingsWithdrawPreviewViewModel preview = await BuildWithdrawPreviewAsync(selectedAccount, amountText);
            return Json(preview);
        }
        catch (Exception)
        {
            return Json(new SavingsWithdrawPreviewViewModel());
        }
    }

    private int CurrentUserId => int.TryParse(User.FindFirst(AuthClaimTypes.UserId)?.Value, out int userId) ? userId : 0;

    private async Task<SavingsPageViewModel> BuildPageModelAsync(
        int? accountId,
        string manageTab,
        SavingsCreateAccountFormModel? createAccount = null)
    {
        List<SavingsAccount> accounts = await savingsRepoProxy.GetSavingsAccountsByUserIdAsync(CurrentUserId);
        List<FundingSourceOption> fundingSources = await savingsRepoProxy.GetFundingSourcesAsync(CurrentUserId);
        SavingsAccount? selectedAccount = accounts.FirstOrDefault(account => account.Id == (accountId ?? accounts.FirstOrDefault()?.Id));

        var model = new SavingsPageViewModel
        {
            Accounts = accounts,
            FundingSources = fundingSources,
            SelectedAccount = selectedAccount,
            ActiveManageTab = manageTab,
            CreateAccount = createAccount ?? new SavingsCreateAccountFormModel
            {
                FundingSourceId = fundingSources.FirstOrDefault()?.Id,
                SelectedFrequency = "OneTime",
            },
            TotalSavedAmount = await savingsPresentationRepoProxy.GetTotalSavedAmount(accounts),
            NumberOfAccountsText = await savingsPresentationRepoProxy.GetNumberOfAccountsText(accounts.Count),
            BestInterestRate = await savingsPresentationRepoProxy.GetBestInterestRate(accounts),
        };

        if (selectedAccount == null)
        {
            return model;
        }

        model.Deposit = new SavingsDepositFormModel
        {
            AccountId = selectedAccount.Id,
            FundingSourceId = fundingSources.FirstOrDefault()?.Id,
        };

        model.Withdraw = new SavingsWithdrawFormModel
        {
            AccountId = selectedAccount.Id,
            DestinationId = fundingSources.FirstOrDefault()?.Id,
        };

        model.CloseDestinationAccounts = await savingsRepoProxy.GetValidTransferDestinationsAsync(selectedAccount.Id, CurrentUserId);
        model.CloseAccount = new SavingsCloseAccountFormModel
        {
            AccountId = selectedAccount.Id,
            DestinationAccountId = model.CloseDestinationAccounts.FirstOrDefault()?.Id ?? 0,
        };

        AutoDeposit? existingAutoDeposit = await savingsRepoProxy.GetAutoDepositAsync(selectedAccount.Id);
        model.AutoDeposit = new SavingsAutoDepositFormModel
        {
            AccountId = selectedAccount.Id,
            Amount = existingAutoDeposit?.Amount.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty,
            Frequency = existingAutoDeposit?.Frequency.ToString() ?? "Monthly",
            StartDate = existingAutoDeposit?.NextRunDate ?? DateTime.Today.AddDays(1),
            IsActive = existingAutoDeposit?.IsActive ?? true,
        };

        model.WithdrawPreview = await BuildWithdrawPreviewAsync(selectedAccount, model.Withdraw.Amount);
        return model;
    }

    private async Task<SavingsWithdrawPreviewViewModel> BuildWithdrawPreviewAsync(SavingsAccount selectedAccount, string amountText)
    {
        var preview = new SavingsWithdrawPreviewViewModel
        {
            HasEarlyRisk = await savingsPresentationRepoProxy.CheckClosePenaltyRisk(selectedAccount),
        };

        if (!preview.HasEarlyRisk)
        {
            return preview;
        }

        preview.PenaltySummary = selectedAccount.MaturityDate.HasValue
            ? $"Early withdrawal penalty applies until {selectedAccount.MaturityDate.Value:dd MMM yyyy}."
            : "Early withdrawal penalty applies to this account.";

        try
        {
            decimal amount = await savingsUiRulesRepoProxy.ParsePositiveAmount(amountText);
            decimal penalty = amount * 0.02m;
            decimal netAmount = await savingsUiRulesRepoProxy.GetWithdrawNetAmount(amount, penalty);
            preview.HasPenalty = penalty > 0m;
            preview.PenaltyBreakdown = $"Penalty: {penalty:C2}";
            preview.NetAmountText = $"Net amount received: {netAmount:C2}";
        }
        catch (InvalidOperationException)
        {
            // Ignore incomplete input and keep the contextual warning only.
        }

        return preview;
    }

    private void AddCreateAccountErrors(Dictionary<string, string> errors)
    {
        foreach ((string key, string value) in errors)
        {
            string modelStateKey = key switch
            {
                "SavingsType" => "CreateAccount.SelectedSavingsType",
                "AccountName" => "CreateAccount.AccountName",
                "InitialDeposit" => "CreateAccount.InitialDeposit",
                "FundingSource" => "CreateAccount.FundingSourceId",
                "Frequency" => "CreateAccount.SelectedFrequency",
                "TargetAmount" => "CreateAccount.TargetAmount",
                "TargetDate" => "CreateAccount.TargetDate",
                _ => string.Empty,
            };

            if (!string.IsNullOrWhiteSpace(modelStateKey))
            {
                ModelState.AddModelError(modelStateKey, value);
            }
        }
    }

    private static void NormalizeCreateAccountForm(
        SavingsCreateAccountFormModel createAccount,
        List<FundingSourceOption> fundingSources)
    {
        createAccount.SelectedSavingsType = createAccount.SelectedSavingsType?.Trim() ?? string.Empty;
        createAccount.AccountName = createAccount.AccountName?.Trim() ?? string.Empty;
        createAccount.InitialDeposit = createAccount.InitialDeposit?.Trim() ?? string.Empty;
        createAccount.TargetAmount = createAccount.TargetAmount?.Trim() ?? string.Empty;
        createAccount.SelectedFrequency = string.IsNullOrWhiteSpace(createAccount.SelectedFrequency)
            ? "OneTime"
            : createAccount.SelectedFrequency.Trim();

        if (!createAccount.FundingSourceId.HasValue && fundingSources.Count > 0)
        {
            createAccount.FundingSourceId = fundingSources[0].Id;
        }

        if (!IsGoalSavings(createAccount.SelectedSavingsType))
        {
            createAccount.TargetAmount = string.Empty;
            createAccount.TargetDate = null;
        }

        if (!IsFixedDeposit(createAccount.SelectedSavingsType))
        {
            createAccount.MaturityDate = null;
        }
    }

    private static bool IsGoalSavings(string? savingsType) =>
        string.Equals(savingsType, "GoalSavings", StringComparison.OrdinalIgnoreCase);

    private static bool IsFixedDeposit(string? savingsType) =>
        string.Equals(savingsType, "FixedDeposit", StringComparison.OrdinalIgnoreCase);
}
