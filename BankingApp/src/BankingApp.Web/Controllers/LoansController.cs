namespace BankingApp.Web.Controllers;

using Contracts.Features.Loans.Dtos;
using Contracts.Http;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;
using Domain.Enums;
using Infrastructure.Http.Features.Loans.Services;
using Models.Loans;
using Microsoft.AspNetCore.Mvc;

public class LoansController(
    ILoansRepoProxy loansRepoProxy,
    ILoanDialogStateRepoProxy loanDialogStateRepoProxy,
    ILoanApplicationPresentationRepoProxy loanApplicationPresentationRepoProxy) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(LoanStatus? statusFilter = null, LoanType? typeFilter = null)
    {
        try
        {
            LoansPageViewModel model = await BuildPageModelAsync(statusFilter, typeFilter);
            return View(model);
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
            return View(new LoansPageViewModel
            {
                SelectedStatusFilter = statusFilter,
                SelectedTypeFilter = typeFilter,
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(
        [Bind(Prefix = "Application")] LoanApplicationFormModel application,
        LoanStatus? statusFilter = null,
        LoanType? typeFilter = null)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please complete the application form.";
                LoansPageViewModel invalidModel = await BuildPageModelAsync(statusFilter, typeFilter, application);
                return View("Index", invalidModel);
            }

            int applicationResult = await loansRepoProxy.CreateLoanApplicationAsync(new LoanApplicationRequest
            {
                UserId = CurrentUserId,
                LoanType = application.LoanType,
                DesiredAmount = application.DesiredAmount,
                PreferredTermMonths = application.PreferredTermMonths,
                Purpose = application.Purpose.Trim(),
            });

            BuildApplicationOutcomeResponse? outcome = await loanApplicationPresentationRepoProxy.GetBuildApplicationOutcome(
                applicationResult > 0 ? null : "Loan application was rejected.");

            TempData[outcome?.IsApproved == true ? "Success" : "Error"] =
                outcome?.Message ?? "Loan application processed.";

            return RedirectToAction(nameof(Index), new { statusFilter, typeFilter });
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
            LoansPageViewModel invalidModel = await BuildPageModelAsync(statusFilter, typeFilter, application);
            return View("Index", invalidModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayInstallment(
        [Bind(Prefix = "Payment")] LoanPaymentFormModel payment,
        LoanStatus? statusFilter = null,
        LoanType? typeFilter = null)
    {
        try
        {
            Loan loan = await loansRepoProxy.GetLoanByIdAsync(payment.LoanId);
            decimal minimumDue = Math.Min(loan.MonthlyInstallment, loan.OutstandingBalance);
            decimal paymentAmount = payment.UseCustomAmount
                ? ParseCustomPaymentAmount(payment.CustomAmount) ?? 0m
                : minimumDue;

            if (paymentAmount <= 0m)
            {
                TempData["Error"] = "Enter a valid custom payment amount.";
                return RedirectToAction(nameof(Index), new { statusFilter, typeFilter });
            }

            decimal newBalance = Math.Max(0m, loan.OutstandingBalance - paymentAmount);
            int monthsPaid = payment.UseCustomAmount && loan.MonthlyInstallment > 0m
                ? Math.Max(1, (int)Math.Floor(paymentAmount / loan.MonthlyInstallment))
                : 1;
            int newRemainingMonths = newBalance <= 0m ? 0 : Math.Max(0, loan.RemainingMonths - monthsPaid);
            LoanStatus newStatus = newBalance <= 0m ? LoanStatus.Passed : loan.LoanStatus;

            await loansRepoProxy.UpdateLoanAfterPaymentAsync(loan.Id, newBalance, newRemainingMonths, newStatus);
            TempData["Success"] = "Installment payment posted successfully.";
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { statusFilter, typeFilter });
    }

    [HttpGet]
    public async Task<IActionResult> Estimate(LoanType loanType, decimal desiredAmount, int preferredTermMonths, string purpose)
    {
        try
        {
            bool shouldCompute = await loanDialogStateRepoProxy.GetShouldComputeEstimate((double)desiredAmount, preferredTermMonths, purpose);
            if (!shouldCompute)
            {
                return Json(new { show = false });
            }

            LoanEstimate estimate = GetLoanEstimate(loanType, desiredAmount, preferredTermMonths);
            return Json(new
            {
                show = true,
                rate = $"{estimate.IndicativeRate:N2}%",
                monthly = $"{estimate.MonthlyInstallment:C2}",
                total = $"{estimate.TotalRepayable:C2}",
            });
        }
        catch (Exception)
        {
            return Json(new { show = false });
        }
    }

    [HttpGet]
    public async Task<IActionResult> PaymentPreview(int loanId, bool useCustomAmount, string? customAmount)
    {
        try
        {
            Loan loan = await loansRepoProxy.GetLoanByIdAsync(loanId);
            decimal minimumDue = Math.Min(loan.MonthlyInstallment, loan.OutstandingBalance);
            decimal paymentAmount = minimumDue;

            if (useCustomAmount)
            {
                decimal? parsedAmount = ParseCustomPaymentAmount(customAmount ?? string.Empty);
                if (!parsedAmount.HasValue)
                {
                    return Json(new LoanPaymentPreviewViewModel { ErrorMessage = "Enter a valid amount." });
                }

                paymentAmount = parsedAmount.Value;
                if (paymentAmount < minimumDue)
                {
                    return Json(new LoanPaymentPreviewViewModel
                    {
                        ErrorMessage = "Custom amount must be at least the amount currently due.",
                    });
                }
            }

            if (paymentAmount > loan.OutstandingBalance)
            {
                return Json(new LoanPaymentPreviewViewModel
                {
                    ErrorMessage = "Payment amount exceeds the outstanding balance.",
                });
            }

            (decimal balanceAfterPayment, int remainingMonths) = CalculatePaymentPreview(loan, useCustomAmount ? paymentAmount : null);
            return Json(new LoanPaymentPreviewViewModel
            {
                BalanceAfterPayment = balanceAfterPayment,
                RemainingMonthsAfterPayment = remainingMonths,
            });
        }
        catch (Exception)
        {
            return Unauthorized();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Schedule(int id)
    {
        try
        {
            Loan loan = await loansRepoProxy.GetLoanByIdAsync(id);
            List<AmortizationRow> rows = await loansRepoProxy.GetAmortizationAsync(id);
            var model = new LoanSchedulePageViewModel
            {
                Loan = new LoanCardViewModel
                {
                    Loan = loan,
                    RepaymentProgress = GetRepaymentProgress(loan),
                },
                Rows = rows,
            };

            return View(model);
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private int CurrentUserId => int.TryParse(User.FindFirst(AuthClaimTypes.UserId)?.Value, out int userId) ? userId : 0;

    private async Task<LoansPageViewModel> BuildPageModelAsync(
        LoanStatus? statusFilter,
        LoanType? typeFilter,
        LoanApplicationFormModel? application = null)
    {
        List<Loan> loans = await loansRepoProxy.GetLoansByUserAsync(CurrentUserId);
        var cards = loans
            .Select(loan => new LoanCardViewModel
            {
                Loan = loan,
                RepaymentProgress = GetRepaymentProgress(loan),
            })
            .Where(card =>
                (!statusFilter.HasValue || card.Loan.LoanStatus == statusFilter.Value) &&
                (!typeFilter.HasValue || card.Loan.LoanType == typeFilter.Value))
            .OrderByDescending(card => card.Loan.StartDate)
            .ToList();

        return new LoansPageViewModel
        {
            Loans = cards,
            Application = application ?? new LoanApplicationFormModel(),
            SelectedStatusFilter = statusFilter,
            SelectedTypeFilter = typeFilter,
        };
    }

    private static LoanEstimate GetLoanEstimate(LoanType loanType, decimal desiredAmount, int preferredTermMonths)
    {
        decimal rate = loanType switch
        {
            LoanType.Mortgage => 4.5m,
            LoanType.Student => 3.0m,
            LoanType.Auto => 6.5m,
            _ => 8.5m,
        };
        decimal monthly = preferredTermMonths <= 0 ? 0m : desiredAmount * (1 + rate / 100m) / preferredTermMonths;
        return new LoanEstimate(rate, monthly, monthly * preferredTermMonths);
    }

    private static double GetRepaymentProgress(Loan loan)
    {
        if (loan.Principal <= 0m)
        {
            return 0d;
        }

        return (double)((loan.Principal - loan.OutstandingBalance) / loan.Principal * 100m);
    }

    private static decimal? ParseCustomPaymentAmount(string customAmount)
    {
        return decimal.TryParse(customAmount, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal amount) && amount > 0m
            ? amount
            : null;
    }

    private static (decimal BalanceAfterPayment, int RemainingMonths) CalculatePaymentPreview(Loan loan, decimal? customAmount)
    {
        const decimal zeroAmount = 0m;
        const int zeroCount = 0;

        decimal minimumDue = Math.Min(loan.MonthlyInstallment, loan.OutstandingBalance);
        decimal paymentAmount = customAmount ?? minimumDue;
        decimal balanceAfterPayment = Math.Max(zeroAmount, loan.OutstandingBalance - paymentAmount);

        if (balanceAfterPayment <= zeroAmount)
        {
            return (zeroAmount, zeroCount);
        }

        int monthsPaid = customAmount.HasValue && loan.MonthlyInstallment > zeroAmount
            ? Math.Max(1, (int)Math.Floor(paymentAmount / loan.MonthlyInstallment))
            : 1;

        return (balanceAfterPayment, Math.Max(zeroCount, loan.RemainingMonths - monthsPaid));
    }
}
