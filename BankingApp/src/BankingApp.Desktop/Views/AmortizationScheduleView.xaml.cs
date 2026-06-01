namespace BankingApp.Desktop.Views;

using System;
using ViewModels;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

public sealed partial class AmortizationScheduleView : Page
{
    private const string LoanHeaderFormat = "{0} · {1} months · {2:0.##}%";
    private const byte CurrentRowHighlightAlpha = 40;
    private const byte CurrentRowHighlightRed = 0;
    private const byte CurrentRowHighlightGreen = 120;
    private const byte CurrentRowHighlightBlue = 215;

    private static readonly SolidColorBrush _currentRowHighlightBrush = new SolidColorBrush(
        ColorHelper.FromArgb(
            CurrentRowHighlightAlpha,
            CurrentRowHighlightRed,
            CurrentRowHighlightGreen,
            CurrentRowHighlightBlue));

    private Loan? _loan;

    public AmortizationScheduleView(LoansViewModel loansViewModel)
    {
        InitializeComponent();

        ViewModel = loansViewModel;
        DataContext = ViewModel;

        // Highlight the current installment row after containers are created.
        AmortizationListView.ContainerContentChanging += OnRowContainerContentChanging;
    }

    private LoansViewModel ViewModel { get; }

    public async void LoadLoan(Loan loan)
    {
        this._loan = loan;
        PopulateStaticLabels(loan);

        ViewModel.SelectLoan(loan);
        await ViewModel.LoadAmortizationAsync();
    }

    private void PopulateStaticLabels(Loan loan)
    {
        LoanSubHeaderText.Text =
            string.Format(LoanHeaderFormat, loan.LoanType, loan.TermInMonths, loan.InterestRate);

        TotalInstallmentsText.Text = loan.TermInMonths.ToString();
        PaidInstallmentsText.Text = (loan.TermInMonths - loan.RemainingMonths).ToString();
        RemainingInstallmentsText.Text = loan.RemainingMonths.ToString();
    }

    private void OnRowContainerContentChanging(
        ListViewBase sender,
        ContainerContentChangingEventArgs args)
    {
        if (args.Item is AmortizationRow row && args.ItemContainer is ListViewItem container)
        {
            container.Background = row.IsCurrent
                ? _currentRowHighlightBrush
                : null;
        }
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private async void OnDownloadPdfClicked(object sender, RoutedEventArgs e)
    {
        if (_loan != null)
        {
            await ViewModel.DownloadSchedulePdfAsync();
        }
    }
}
