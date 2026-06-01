namespace BankingApp.Desktop.Views;

using System;
using System.Threading.Tasks;
using Contracts.Features.Investments;
using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Domain.Aggregates.SavingsAggregate;
using Domain.Aggregates.UserAggregate;
using Session;

public sealed partial class SavingsView
{
    private const int FirstItemIndex = 0;
    private const int NoSelectionIndex = -1;
    private const int SuccessMessageDelayMilliseconds = 1500;
    private const string OneTimeFrequencyTag = "OneTime";
    private readonly IAuthenticationSession _authenticationSession;

    public SavingsViewModel? ViewModel => DataContext as SavingsViewModel;

    public SavingsView(IAuthenticationSession authenticationSession)
    {
        _authenticationSession = authenticationSession;
        InitializeComponent();
        MainNavigationView.SelectedItem = MyAccountsTab;
        Loaded += SavingsView_Loaded;
    }

    private async void SavingsView_Loaded(object sender, RoutedEventArgs e)
    {
        // Ne asigurăm că ViewModel-ul a fost injectat cu succes din XAML
        if (ViewModel != null)
        {
            try
            {
                int userId = _authenticationSession.CurrentUserId ?? throw new Exception("Current user id is null.");
                ViewModel.CurrentUser = new User { Id = userId };

                await ViewModel.LoadAccountsAsync();
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message;
            }

            if (ViewModel.HasError)
            {
                await ShowDialogAsync("Load Error", ViewModel.ErrorMessage);
            }
        }
    }

    private async void OnTabSelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem tab)
        {
            return;
        }

        string tag = tab.Tag?.ToString() ?? string.Empty;

        MyAccountsPanel.Visibility = tag == "MyAccounts" ? Visibility.Visible : Visibility.Collapsed;
        OpenNewPanel.Visibility = tag == "OpenNew" ? Visibility.Visible : Visibility.Collapsed;
        ManagePanel.Visibility = tag == "Manage" ? Visibility.Visible : Visibility.Collapsed;

        if (tag == "OpenNew")
        {
            ResetCreateFormControls();
            ViewModel?.SelectedSavingsType = string.Empty;

            await ViewModel?.LoadFundingSourcesAsync()!;
            FundingSourceComboBox.ItemsSource = ViewModel.FundingSources;
            if (ViewModel.FundingSources.Any())
            {
                FundingSourceOption defaultFundingSource = ViewModel.FundingSources
                                                               .FirstOrDefault(source => source.Id == ViewModel.SelectedFundingSource?.Id)
                                                           ?? ViewModel.FundingSources[FirstItemIndex];
                FundingSourceComboBox.SelectedItem = defaultFundingSource;
                ViewModel.SelectedFundingSource = defaultFundingSource;
            }
        }

        if (tag != "Manage")
        {
            return;
        }

        HideAllActionPanels();
        ManageButtonsPanel.Visibility = Visibility.Collapsed;
        ManageAccountComboBox.SelectedIndex = NoSelectionIndex;
    }

    private void OnFrequencySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FrequencyRadioButtons.SelectedItem is RadioButton radioButton)
        {
            ViewModel.SelectedFrequency = radioButton.Tag?.ToString() ?? string.Empty;
        }
    }

    private void OnSavingsTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SavingsTypeRadioButtons.SelectedItem is not RadioButton radioButton)
        {
            return;
        }

        ViewModel.SelectedSavingsType = radioButton.Tag?.ToString() ?? string.Empty;

        GoalSavingsPanel.Visibility = ViewModel.SelectedSavingsType == "GoalSavings"
            ? Visibility.Visible
            : Visibility.Collapsed;

        FixedDepositPanel.Visibility = ViewModel.SelectedSavingsType == "FixedDeposit"
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (ViewModel.SelectedSavingsType != "GoalSavings")
        {
            TargetAmountTextBox.Text = string.Empty;
            TargetDatePicker.Date = null;
            TargetAmountError.Visibility = Visibility.Collapsed;
            TargetDateError.Visibility = Visibility.Collapsed;
        }

        if (ViewModel.SelectedSavingsType != "FixedDeposit")
        {
            MaturityDatePicker.Date = null;
        }
    }

    private async void OnOpenAccountClicked(object sender, RoutedEventArgs e)
    {
        ClearCreateErrors();
        ViewModel.PrepareCreateAccountSubmission(
            GetSelectedRadioButtonTag(SavingsTypeRadioButtons),
            GetSelectedRadioButtonTag(FrequencyRadioButtons),
            AccountNameTextBox.Text,
            InitialDepositTextBox.Text,
            FundingSourceComboBox.SelectedItem as FundingSourceOption,
            TargetAmountTextBox.Text,
            TargetDatePicker.Date,
            MaturityDatePicker.Date);

        await ViewModel.CreateAccountCommand.ExecuteAsync(null);

        if (ViewModel.FieldErrors.TryGetValue("SavingsType", out string? savingsTypeError))
        {
            ShowError(TypeErrorText, savingsTypeError);
        }

        if (ViewModel.FieldErrors.TryGetValue("AccountName", out string? accountNameError))
        {
            ShowError(AccountNameError, accountNameError);
        }

        if (ViewModel.FieldErrors.TryGetValue("InitialDeposit", out string? initialDepositError))
        {
            ShowError(InitialDepositError, initialDepositError);
        }

        if (ViewModel.FieldErrors.TryGetValue("FundingSource", out string? fundingSourceError))
        {
            ShowError(FundingSourceError, fundingSourceError);
        }

        if (ViewModel.FieldErrors.TryGetValue("Frequency", out string? frequencyError))
        {
            ShowError(FrequencyError, frequencyError);
        }

        if (ViewModel.FieldErrors.TryGetValue("TargetAmount", out string? targetAmountError))
        {
            ShowError(TargetAmountError, targetAmountError);
        }

        if (ViewModel.FieldErrors.TryGetValue("TargetDate", out string? targetDateError))
        {
            ShowError(TargetDateError, targetDateError);
        }

        if (ViewModel.HasError)
        {
            CreateErrorBar.Message = ViewModel.ErrorMessage;
            CreateErrorBar.IsOpen = true;
            return;
        }

        if (!ViewModel.ShowCreateConfirmation)
        {
            return;
        }

        CreateSuccessBar.IsOpen = true;
        OpenAccountButton.IsEnabled = false;
        await Task.Delay(SuccessMessageDelayMilliseconds);
        CreateSuccessBar.IsOpen = false;
        OpenAccountButton.IsEnabled = true;
        ResetCreateFormControls();
        MainNavigationView.SelectedItem = MyAccountsTab;
    }

    private void OnCancelCreateClicked(object sender, RoutedEventArgs e)
    {
        MainNavigationView.SelectedItem = MyAccountsTab;
    }

    private void OnManageAccountSelected(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectedAccount = ManageAccountComboBox.SelectedItem as SavingsAccount;
        HideAllActionPanels();
        ManageButtonsPanel.Visibility = ViewModel.SelectedAccount != null
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async void OnDepositClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedAccount == null)
        {
            return;
        }

        // Load funding sources into the deposit combobox
        await ViewModel.LoadFundingSourcesAsync();
        DepositSourceComboBox.ItemsSource = ViewModel.FundingSources;
        if (ViewModel.FundingSources.Any())
        {
            FundingSourceOption defaultFundingSource = ViewModel.FundingSources
                                                           .FirstOrDefault(source => source.Id == ViewModel.SelectedFundingSource?.Id)
                                                       ?? ViewModel.FundingSources[FirstItemIndex];
            DepositSourceComboBox.SelectedItem = defaultFundingSource;
            ViewModel.DepositSource = defaultFundingSource.DisplayName;
        }

        // Sync amount field
        DepositAmountTextBox.Text = string.Empty;
        ViewModel.DepositAmountText = string.Empty;
        DepositLivePreview.Text = string.Empty;
        DepositResultBar.IsOpen = false;

        HideAllActionPanels();
        ManageButtonsPanel.Visibility = Visibility.Collapsed;
        DepositActionPanel.Visibility = Visibility.Visible;
    }

    private async void OnWithdrawClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedAccount == null)
        {
            return;
        }

        // Load funding sources as withdraw destinations
        await ViewModel.LoadFundingSourcesAsync();
        WithdrawDestComboBox.ItemsSource = ViewModel.FundingSources;
        if (ViewModel.FundingSources.Any())
        {
            FundingSourceOption defaultFundingSource = ViewModel.FundingSources
                                                           .FirstOrDefault(source => source.Id == ViewModel.SelectedFundingSource?.Id)
                                                       ?? ViewModel.FundingSources[FirstItemIndex];
            WithdrawDestComboBox.SelectedItem = defaultFundingSource;
            ViewModel.WithdrawDestination = defaultFundingSource;
        }

        WithdrawAmountTextBox.Text = string.Empty;
        ViewModel.WithdrawAmountText = string.Empty;
        WithdrawResultBar.IsOpen = false;

        // Show penalty warning if applicable
        WithdrawPenaltyWarning.Visibility = ViewModel.WithdrawHasEarlyRisk
            ? Visibility.Visible
            : Visibility.Collapsed;
        WithdrawPenaltySummaryText.Text = ViewModel.WithdrawPenaltySummary;
        WithdrawPenaltyBreakdown.Visibility = Visibility.Collapsed;

        HideAllActionPanels();
        ManageButtonsPanel.Visibility = Visibility.Collapsed;
        WithdrawActionPanel.Visibility = Visibility.Visible;
    }

    private async void OnAutoDepositClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedAccount == null)
        {
            return;
        }

        await ViewModel.LoadAutoDepositAsync(ViewModel.SelectedAccount.IdentificationNumber);

        AutoDepositTitle.Text = ViewModel.ExistingLabel + " Auto Deposit";
        AutoDepositAmountTextBox.Text = ViewModel.AutoDepositAmountText;

        // Set frequency radio
        AutoDepositFrequencyRadios.SelectedIndex = NoSelectionIndex;
        for (int i = FirstItemIndex; i < AutoDepositFrequencyRadios.Items.Count; i++)
        {
            if (AutoDepositFrequencyRadios.Items[i] is RadioButton radioButton &&
                radioButton.Tag?.ToString() == ViewModel.AutoDepositFrequency)
            {
                AutoDepositFrequencyRadios.SelectedIndex = i;
                break;
            }
        }

        AutoDepositStartDatePicker.Date = ViewModel.AutoDepositStartDate;
        AutoDepositActiveToggle.IsOn = ViewModel.AutoDepositIsActive;
        AutoDepositResultBar.IsOpen = false;

        HideAllActionPanels();
        ManageButtonsPanel.Visibility = Visibility.Collapsed;
        AutoDepositActionPanel.Visibility = Visibility.Visible;
    }

    private async void OnCloseAccountClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedAccount == null)
        {
            return;
        }

        await ViewModel.LoadCloseDestinationAccountsAsync();

        CloseDestComboBox.ItemsSource = ViewModel.CloseDestinationAccounts;
        CloseResultBar.IsOpen = false;
        CloseConfirmCheckBox.IsChecked = false;
        CloseConfirmButton.IsEnabled = false;

        bool hasNoDest = !ViewModel.CloseDestinationAccounts.Any();
        CloseNoDestText.Visibility = hasNoDest ? Visibility.Visible : Visibility.Collapsed;
        CloseDestComboBox.Visibility = hasNoDest ? Visibility.Collapsed : Visibility.Visible;

        if (!hasNoDest)
        {
            CloseDestComboBox.SelectedIndex = FirstItemIndex;
        }

        // Show penalty warning for fixed deposit before maturity
        ClosePenaltyWarning.Visibility = ViewModel.CloseHasPenalty
            ? Visibility.Visible
            : Visibility.Collapsed;

        HideAllActionPanels();
        ManageButtonsPanel.Visibility = Visibility.Collapsed;
        CloseAccountActionPanel.Visibility = Visibility.Visible;
    }

    private void OnDepositAmountChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.DepositAmountText = DepositAmountTextBox.Text;
        DepositLivePreview.Text = ViewModel.LivePreview;
    }

    private void OnDepositSourceChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DepositSourceComboBox.SelectedItem is FundingSourceOption fundingSourceOption)
        {
            ViewModel.DepositSource = fundingSourceOption.DisplayName;
        }
    }

    private async void OnDepositConfirmed(object sender, RoutedEventArgs e)
    {
        DepositResultBar.IsOpen = false;
        await ViewModel.DepositAsync();

        if (ViewModel.HasError)
        {
            DepositResultBar.Severity = InfoBarSeverity.Error;
            DepositResultBar.Message = ViewModel.ErrorMessage;
            DepositResultBar.IsOpen = true;
        }
        else if (ViewModel.ShowDepositSuccess)
        {
            DepositResultBar.Severity = InfoBarSeverity.Success;
            DepositResultBar.Message = ViewModel.DepositSuccessMessage;
            DepositResultBar.IsOpen = true;
            DepositAmountTextBox.Text = string.Empty;
        }
    }

    private void OnDepositBack(object sender, RoutedEventArgs e)
    {
        DepositActionPanel.Visibility = Visibility.Collapsed;
        ManageButtonsPanel.Visibility = Visibility.Visible;
    }

    private void OnWithdrawAmountChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.WithdrawAmountText = WithdrawAmountTextBox.Text;

        bool hasPenalty = ViewModel.WithdrawHasPenalty;
        WithdrawPenaltyBreakdown.Visibility = hasPenalty ? Visibility.Visible : Visibility.Collapsed;
        if (hasPenalty)
        {
            WithdrawPenaltyAmountText.Text = ViewModel.WithdrawPenaltyBreakdownText;
            WithdrawNetAmountText.Text = ViewModel.WithdrawNetAmountText;
        }
    }

    private void OnWithdrawDestChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.WithdrawDestination = WithdrawDestComboBox.SelectedItem as FundingSourceOption;
    }

    private async void OnWithdrawConfirmed(object sender, RoutedEventArgs e)
    {
        WithdrawResultBar.IsOpen = false;
        bool success = await ViewModel.ConfirmWithdrawAsync();

        WithdrawResultBar.Severity = success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
        WithdrawResultBar.Message = ViewModel.WithdrawResultMessage;
        WithdrawResultBar.IsOpen = true;

        if (success)
        {
            WithdrawAmountTextBox.Text = string.Empty;
        }
    }

    private void OnWithdrawBack(object sender, RoutedEventArgs e)
    {
        WithdrawActionPanel.Visibility = Visibility.Collapsed;
        ManageButtonsPanel.Visibility = Visibility.Visible;
    }

    private void OnAutoDepositFrequencyChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AutoDepositFrequencyRadios.SelectedItem is RadioButton radioButton)
        {
            ViewModel.AutoDepositFrequency = radioButton.Tag?.ToString() ?? string.Empty;
        }
    }

    private async void OnAutoDepositSaved(object sender, RoutedEventArgs e)
    {
        AutoDepositResultBar.IsOpen = false;

        ViewModel.AutoDepositAmountText = AutoDepositAmountTextBox.Text;
        ViewModel.AutoDepositStartDate = AutoDepositStartDatePicker.Date;
        ViewModel.AutoDepositIsActive = AutoDepositActiveToggle.IsOn;

        await ViewModel.SaveAutoDepositAsync();

        if (ViewModel.HasError)
        {
            AutoDepositResultBar.Severity = InfoBarSeverity.Error;
            AutoDepositResultBar.Message = ViewModel.ErrorMessage;
            AutoDepositResultBar.IsOpen = true;
        }
        else if (!string.IsNullOrEmpty(ViewModel.AutoDepositSaveMessage))
        {
            AutoDepositResultBar.Severity = InfoBarSeverity.Success;
            AutoDepositResultBar.Message = ViewModel.AutoDepositSaveMessage;
            AutoDepositResultBar.IsOpen = true;
            AutoDepositTitle.Text = "Modify Auto Deposit";
        }
    }

    private void OnAutoDepositBack(object sender, RoutedEventArgs e)
    {
        AutoDepositActionPanel.Visibility = Visibility.Collapsed;
        ManageButtonsPanel.Visibility = Visibility.Visible;
    }

    private void OnCloseDestChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CloseDestComboBox.SelectedItem is SavingsAccount savingsAccount)
        {
            ViewModel.SelectedCloseDestinationId = savingsAccount.IdentificationNumber;
        }
    }

    private void OnCloseConfirmChecked(object sender, RoutedEventArgs e)
    {
        ViewModel.CloseUserConfirmed = true;
        CloseConfirmButton.IsEnabled = true;
    }

    private void OnCloseConfirmUnchecked(object sender, RoutedEventArgs e)
    {
        ViewModel.CloseUserConfirmed = false;
        CloseConfirmButton.IsEnabled = false;
    }

    private async void OnCloseConfirmed(object sender, RoutedEventArgs e)
    {
        CloseResultBar.IsOpen = false;
        bool success = await ViewModel.ConfirmCloseAsync();

        CloseResultBar.Severity = success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
        CloseResultBar.Message = ViewModel.CloseResultMessage;
        CloseResultBar.IsOpen = true;

        if (success)
        {
            // After successful close, go back to buttons panel after a brief moment
            await Task.Delay(SuccessMessageDelayMilliseconds);
            CloseAccountActionPanel.Visibility = Visibility.Collapsed;
            ManageButtonsPanel.Visibility = Visibility.Visible;
            ManageAccountComboBox.SelectedIndex = NoSelectionIndex;
            ManageButtonsPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void OnCloseBack(object sender, RoutedEventArgs e)
    {
        CloseAccountActionPanel.Visibility = Visibility.Collapsed;
        ManageButtonsPanel.Visibility = Visibility.Visible;
    }

    private void HideAllActionPanels()
    {
        DepositActionPanel.Visibility = Visibility.Collapsed;
        WithdrawActionPanel.Visibility = Visibility.Collapsed;
        AutoDepositActionPanel.Visibility = Visibility.Collapsed;
        CloseAccountActionPanel.Visibility = Visibility.Collapsed;
    }

    private void ClearCreateErrors()
    {
        CreateErrorBar.IsOpen = false;
        CreateSuccessBar.IsOpen = false;
        TypeErrorText.Visibility = Visibility.Collapsed;
        AccountNameError.Visibility = Visibility.Collapsed;
        InitialDepositError.Visibility = Visibility.Collapsed;
        FundingSourceError.Visibility = Visibility.Collapsed;
        FrequencyError.Visibility = Visibility.Collapsed;
        TargetAmountError.Visibility = Visibility.Collapsed;
        TargetDateError.Visibility = Visibility.Collapsed;
    }

    private static string GetSelectedRadioButtonTag(RadioButtons radioButtons)
    {
        return (radioButtons.SelectedItem as RadioButton)?.Tag?.ToString() ?? string.Empty;
    }

    private void ResetCreateFormControls()
    {
        AccountNameTextBox.Text = string.Empty;
        InitialDepositTextBox.Text = string.Empty;
        TargetAmountTextBox.Text = string.Empty;
        TargetDatePicker.Date = null;
        MaturityDatePicker.Date = null;
        SavingsTypeRadioButtons.SelectedIndex = NoSelectionIndex;
        if (ViewModel != null)
        {
            ViewModel.SelectedSavingsType = string.Empty;
        }
        SetDefaultCreateFrequency();
        GoalSavingsPanel.Visibility = Visibility.Collapsed;
        FixedDepositPanel.Visibility = Visibility.Collapsed;
        ClearCreateErrors();
    }

    private void SetDefaultCreateFrequency()
    {
        if (ViewModel == null)
        {
            return;
        }

        FrequencyRadioButtons.SelectedIndex = FirstItemIndex;
        ViewModel.SelectedFrequency = OneTimeFrequencyTag;
    }

    private static void ShowError(TextBlock tb, string msg)
    {
        tb.Text = msg;
        tb.Visibility = Visibility.Visible;
    }

    private async Task ShowDialogAsync(string title, string msg)
    {
        var contentDialog = new ContentDialog
        {
            Title = title,
            Content = msg,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
        };
        await contentDialog.ShowAsync();
    }
}
