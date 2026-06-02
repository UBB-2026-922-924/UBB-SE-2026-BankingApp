using System;
using System.Linq;
using BankingApp.Contracts.Features.Loans.Dtos;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Aggregates.LoanAggregate;
using BankingApp.Web.Infrastructure;
using BankingApp.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Web.Controllers;

using Application.Features.Loans.Services;
using Domain.Aggregates.LoanAggregate.Entities;
using Models.Loans;

//[Authorize]
public class LoansController : WebControllerBase
{
    private readonly ILoansService _loansService;

    public LoansController(ILoansService loansService, IWebSessionContext sessionContext)
        : base(sessionContext)
    {
        _loansService = loansService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(LoanStatus? statusFilter = null, LoanType? typeFilter = null)
    {
        try
        {
            LoansPageViewModel model = await BuildPageModelAsync(statusFilter, typeFilter);
            return View(model);
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            return View(new LoansPageViewModel
            {
                SelectedStatusFilter = statusFilter,
                SelectedTypeFilter = typeFilter,
                ErrorMessage = exception.Message,
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
                LoansPageViewModel invalidModel = await BuildPageModelAsync(statusFilter, typeFilter, application);
                invalidModel.ErrorMessage = "Please complete the application form.";
                return View("Index", invalidModel);
            }

            LoanApplicationResult result = await _loansService.SubmitLoanApplicationAsync(new LoanApplicationRequest
            {
                UserId = CurrentUserId,
                LoanType = application.LoanType,
                DesiredAmount = application.DesiredAmount,
                PreferredTermMonths = application.PreferredTermMonths,
                Purpose = application.Purpose.Trim(),
            });

            BuildApplicationOutcomeResponse? outcome = await _loansService.GetBuildApplicationOutcomeAsync(result.RejectionReason);
            TempData[outcome?.IsApproved == true ? "StatusMessage" : "ErrorMessage"] =
                outcome?.Message ?? "Loan application processed.";

            return RedirectToAction(nameof(Index), new { statusFilter, typeFilter });
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            LoansPageViewModel invalidModel = await BuildPageModelAsync(statusFilter, typeFilter, application);
            invalidModel.ErrorMessage = exception.Message;
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
            decimal? customAmount = null;
            if (payment.UseCustomAmount)
            {
                customAmount = _loansService.ParseCustomPaymentAmount(payment.CustomAmount);
                if (!customAmount.HasValue)
                {
                    TempData["ErrorMessage"] = "Enter a valid custom payment amount.";
                    return RedirectToAction(nameof(Index), new { statusFilter, typeFilter });
                }
            }

            await _loansService.PayInstallmentAsync(payment.LoanId, customAmount);
            TempData["StatusMessage"] = "Installment payment posted successfully.";
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { statusFilter, typeFilter });
    }

    [HttpGet]
    public async Task<IActionResult> Estimate(LoanType loanType, decimal desiredAmount, int preferredTermMonths, string purpose)
    {
        try
        {
            bool shouldCompute = await _loansService.GetShouldComputeEstimateAsync((double)desiredAmount, preferredTermMonths, purpose);
            if (!shouldCompute)
            {
                return Json(new { show = false });
            }

            LoanEstimate estimate = _loansService.GetLoanEstimate(new LoanApplicationRequest
            {
                UserId = CurrentUserId,
                LoanType = loanType,
                DesiredAmount = desiredAmount,
                PreferredTermMonths = preferredTermMonths,
                Purpose = purpose,
            });

            return Json(new
            {
                show = true,
                rate = $"{estimate.IndicativeRate:N2}%",
                monthly = $"{estimate.MonthlyInstallment:C2}",
                total = $"{estimate.TotalRepayable:C2}",
            });
        }
        catch (HttpRequestException)
        {
            return Unauthorized();
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
            Loan? loan = (await _loansService.GetLoansByUserAsync(CurrentUserId))
                .FirstOrDefault(item => item.Id == loanId);

            if (loan == null)
            {
                return NotFound();
            }

            decimal paymentAmount;
            decimal minimumDue = Math.Min(loan.MonthlyInstallment, loan.OutstandingBalance);
            if (useCustomAmount)
            {
                decimal? parsedAmount = _loansService.ParseCustomPaymentAmount(customAmount ?? string.Empty);
                if (!parsedAmount.HasValue)
                {
                    return Json(new LoanPaymentPreviewViewModel
                    {
                        ErrorMessage = "Enter a valid amount.",
                    });
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
            else
            {
                paymentAmount = minimumDue;
            }

            if (paymentAmount > loan.OutstandingBalance)
            {
                return Json(new LoanPaymentPreviewViewModel
                {
                    ErrorMessage = "Payment amount exceeds the outstanding balance.",
                });
            }

            (decimal BalanceAfterPayment, int RemainingMonths) preview = CalculatePaymentPreview(loan, useCustomAmount ? paymentAmount : null);
            return Json(new LoanPaymentPreviewViewModel
            {
                BalanceAfterPayment = preview.BalanceAfterPayment,
                RemainingMonthsAfterPayment = preview.RemainingMonths,
            });
        }
        catch (HttpRequestException)
        {
            return Unauthorized();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Schedule(int id)
    {
        try
        {
            Loan? loan = (await _loansService.GetLoansByUserAsync(CurrentUserId))
                .FirstOrDefault(item => item.Id == id);

            if (loan == null)
            {
                TempData["ErrorMessage"] = "Loan not found.";
                return RedirectToAction(nameof(Index));
            }

            List<AmortizationRow> rows = await _loansService.GetAmortizationAsync(id);
            var model = new LoanSchedulePageViewModel
            {
                Loan = new LoanCardViewModel
                {
                    Loan = loan,
                    RepaymentProgress = _loansService.GetRepaymentProgress(loan),
                },
                Rows = rows,
            };

            return View(model);
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<LoansPageViewModel> BuildPageModelAsync(
        LoanStatus? statusFilter,
        LoanType? typeFilter,
        LoanApplicationFormModel? application = null)
    {
        List<Loan> loans = await _loansService.GetLoansByUserAsync(CurrentUserId);
        var cards = loans
            .Select(loan => new LoanCardViewModel
            {
                Loan = loan,
                RepaymentProgress = _loansService.GetRepaymentProgress(loan),
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
            StatusMessage = TempData["StatusMessage"] as string,
            ErrorMessage = TempData["ErrorMessage"] as string,
        };
    }

    private static (decimal BalanceAfterPayment, int RemainingMonths) CalculatePaymentPreview(Loan loan, decimal? customAmount)
    {
        const decimal ZeroAmount = 0m;
        const int ZeroCount = 0;

        decimal minimumDue = Math.Min(loan.MonthlyInstallment, loan.OutstandingBalance);
        decimal paymentAmount = customAmount ?? minimumDue;
        decimal balanceAfterPayment = Math.Max(ZeroAmount, loan.OutstandingBalance - paymentAmount);

        if (balanceAfterPayment <= ZeroAmount)
        {
            return (ZeroAmount, ZeroCount);
        }

        int monthsPaid = customAmount.HasValue
            ? paymentAmount <= ZeroAmount
                ? ZeroCount
                : (int)Math.Floor(paymentAmount / loan.MonthlyInstallment)
            : 1;

        return (balanceAfterPayment, Math.Max(ZeroCount, loan.RemainingMonths - monthsPaid));
    }
}
