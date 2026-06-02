namespace BankingApp.Desktop.Views;

using System;
using System.Diagnostics;
using ViewModels;
using Domain.Enums;
using Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Navigation;
using Session;
using PayInstallmentDialog = Dialogs.PayInstallmentDialog;

/// <summary>
///     Displays loan applications, payments, and schedules.
/// </summary>
public sealed partial class LoansView : UserControl
{
    private readonly IAuthenticationSession _authenticationSession;
    private readonly IAppNavigationService _navigationService;

    internal LoansViewModel ViewModel => (DataContext as LoansViewModel)!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoansView" /> class.
    /// </summary>
    /// <param name="authenticationSession">The current authentication session.</param>
    /// <param name="navigationService">The application navigation service.</param>
    public LoansView(
        IAuthenticationSession authenticationSession,
        IAppNavigationService navigationService)
    {
        this._authenticationSession = authenticationSession;
        this._navigationService = navigationService;
        InitializeComponent();
        Loaded += LoansView_Loaded;
    }

    private async void LoansView_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            try
            {
                int userId = _authenticationSession.CurrentUserId ?? throw new InvalidOperationException("Current user id is null.");
                ViewModel.CurrentUserId = userId;

                await ViewModel.LoadLoansAsync();
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message;
            }
        }
    }

    private async void OnApplyClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new LoanApplicationDialog(ViewModel)
            {
                XamlRoot = XamlRoot,
            };
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private async void OnPayClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.Tag is LoanViewModel loan)
            {
                ViewModel.SelectedLoan = loan;
                var dialog = new PayInstallmentDialog(ViewModel)
                {
                    XamlRoot = XamlRoot,
                };
                await dialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private async void OnScheduleClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.Tag is LoanViewModel loan)
            {
                ViewModel.SelectedLoan = loan;
                await ViewModel.LoadAmortizationAsync();
                _navigationService.NavigateToContent<AmortizationScheduleView>(view => view.LoadLoan(loan.Loan));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private void OnFilterAll(object sender, RoutedEventArgs e)
    {
        ViewModel.StatusFilter = null;
    }

    private void OnFilterActive(object sender, RoutedEventArgs e)
    {
        ViewModel.StatusFilter = LoanStatus.Active;
    }

    private void OnFilterClosed(object sender, RoutedEventArgs e)
    {
        ViewModel.StatusFilter = LoanStatus.Passed;
    }

    private void OnTypeFilterAll(object sender, RoutedEventArgs e)
    {
        ViewModel.TypeFilter = null;
    }

    private void OnTypeFilterPersonal(object sender, RoutedEventArgs e)
    {
        ViewModel.TypeFilter = LoanType.Personal;
    }

    private void OnTypeFilterMortgage(object sender, RoutedEventArgs e)
    {
        ViewModel.TypeFilter = LoanType.Mortgage;
    }

    private void OnTypeFilterStudent(object sender, RoutedEventArgs e)
    {
        ViewModel.TypeFilter = LoanType.Student;
    }

    private void OnTypeFilterAuto(object sender, RoutedEventArgs e)
    {
        ViewModel.TypeFilter = LoanType.Auto;
    }
}
