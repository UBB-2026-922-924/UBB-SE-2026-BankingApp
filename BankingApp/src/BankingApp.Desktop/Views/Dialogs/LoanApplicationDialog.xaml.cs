namespace BankingApp.Desktop.Views.Dialogs;

using ViewModels;
using Microsoft.UI.Xaml.Controls;

/// <summary>
///     Dialog used to review and submit a loan application.
/// </summary>
public sealed partial class LoanApplicationDialog : ContentDialog
{
    private readonly LoansViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoanApplicationDialog" /> class.
    /// </summary>
    /// <param name="viewModel">The loans view model.</param>
    public LoanApplicationDialog(LoansViewModel viewModel)
    {
        InitializeComponent();
        this._viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void OnSubmitClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        ContentDialogButtonClickDeferral? deferral = args.GetDeferral();
        if (!_viewModel.IsReviewVisible)
        {
            args.Cancel = true;
            _viewModel.SwitchToReviewStage();
            sender.Title = _viewModel.DialogTitle;
            sender.PrimaryButtonText = _viewModel.DialogActionText;
        }
        else
        {
            await _viewModel.ApplyForLoanAsync();

            if (!string.IsNullOrEmpty(_viewModel.ApplicationResult))
            {
                ResultBar.Message = _viewModel.ApplicationResult;
                ResultBar.Severity = _viewModel.ApplicationWasApproved
                    ? InfoBarSeverity.Success
                    : InfoBarSeverity.Error;
                ResultBar.IsOpen = true;
                args.Cancel = true;
            }
        }

        deferral.Complete();
    }
}
