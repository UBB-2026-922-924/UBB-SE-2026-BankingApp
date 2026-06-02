namespace BankingApp.Desktop.Views.Dialogs;

using System;
using System.Globalization;
using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>
///     Dialog used to confirm a loan installment payment.
/// </summary>
public sealed partial class PayInstallmentDialog : ContentDialog
{
    private readonly LoansViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PayInstallmentDialog" /> class.
    /// </summary>
    /// <param name="viewModel">The loans view model.</param>
    public PayInstallmentDialog(LoansViewModel viewModel)
    {
        InitializeComponent();
        this._viewModel = viewModel;
        DataContext = viewModel;
        UpdatePreview();
    }

    private async void OnConfirmClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        ContentDialogButtonClickDeferral? deferral = args.GetDeferral();
        try
        {
            await _viewModel.PayInstallmentAsync();
        }
        catch (Exception)
        {
            args.Cancel = true;
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void OnStandardChecked(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            return;
        }

        if (CustomAmountPanel != null)
        {
            CustomAmountPanel.Visibility = Visibility.Collapsed;
        }

        _viewModel.SelectStandardPayment();
        UpdatePreview();
    }

    private void OnCustomChecked(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            return;
        }

        CustomAmountPanel.Visibility = Visibility.Visible;
        if (_viewModel.SelectedLoan != null)
        {
            CustomAmountBox.Text = _viewModel.SelectCustomPayment();
        }

        UpdatePreview();
    }

    private void OnCustomAmountTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdatePreview();
    }

    private void OnCustomAmountLostFocus(object sender, RoutedEventArgs e)
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_viewModel == null)
        {
            return;
        }

        if (_viewModel.SelectedLoan == null)
        {
            BalanceAfterPaymentText.Text = string.Empty;
            RemainingTermAfterPaymentText.Text = string.Empty;
            return;
        }

        if (StandardRadio.IsChecked == true)
        {
            _viewModel.SelectStandardPayment();
        }
        else
        {
            _viewModel.UpdateCustomPayment(CustomAmountBox?.Text ?? string.Empty);
        }

        BalanceAfterPaymentText.Text = _viewModel.PaymentPreviewBalance.ToString("C2", CultureInfo.CurrentCulture);
        RemainingTermAfterPaymentText.Text = $"{_viewModel.PaymentPreviewRemainingMonths} mo";
    }
}
