namespace BankingApp.Desktop.Views;

using System;
using System.Threading.Tasks;
using Contracts.Features.UserProfile.Dtos;
using ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Navigation;
using Serilog;
using Shared.Enums;

/// <summary>
///     Displays and manages the authenticated user's profile settings.
/// </summary>
public sealed partial class ProfileView
{
    private const double EnabledFormOpacity = 1.0;
    private const double DisabledFormOpacity = 0.6;
    private const byte OpaqueColorAlpha = 255;
    private const byte PrimaryTextRed = 30;
    private const byte PrimaryTextGreen = 41;
    private const byte PrimaryTextBlue = 59;

    private readonly IAppNavigationService _navigationService;
    private readonly ProfileViewModel _viewModel;
    private bool _isChangingPasswordFlow;
    private bool _isUpdatingToggle;
    private string _verifiedPassword = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that loads profile data and drives all profile update operations.</param>
    /// <param name="navigationService">Used to navigate to the dashboard or back to login.</param>
    public ProfileView(ProfileViewModel viewModel, IAppNavigationService navigationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navigationService = navigationService;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Loaded += OnPageLoaded;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProfileViewModel.State))
        {
            OnStateChanged(_viewModel.State);
        }
    }

    private void OnStateChanged(ProfileState state)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Log.Information("ProfileView state changed to {State}.", state);
            if (_isUpdatingToggle)
            {
                if (state == ProfileState.Error)
                {
                    ShowError("Failed to save notification preferences.");
                }

                return;
            }

            switch (state)
            {
                case ProfileState.Loading:
                    ShowLoading(true);
                    break;
                case ProfileState.UpdateSuccess:
                    ShowLoading(false);
                    TryPopulateUi("state-update");
                    break;
                case ProfileState.Error:
                    ShowLoading(false);
                    ShowError("Operation failed.");
                    break;
                case ProfileState.Idle:
                case ProfileState.Success:
                case ProfileState.PasswordChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        });
    }

    /// <inheritdoc />
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        ShowLoading(true);
        try
        {
            Log.Information("ProfileView load started.");
            bool loaded = await _viewModel.LoadProfile();
            Log.Information(
                "ProfileView load finished. Success={Loaded}, UserId={UserId}, PreferencesCount={PreferencesCount}.",
                loaded,
                _viewModel.ProfileDto.UserId,
                _viewModel.Notifications.NotificationPreferences.Count);
            ShowLoading(false);
            if (!loaded)
            {
                ShowError("Failed to load profile.");
                return;
            }

            TryPopulateUi("page-load");
            SetEditingEnabled(false);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "ProfileView load crashed.");
            ShowLoading(false);
            ShowError($"Failed to load profile: {exception.Message}");
        }
    }

    private void TryPopulateUi(string trigger)
    {
        try
        {
            PopulateUi();
        }
        catch (Exception exception)
        {
            Log.Error(
                exception,
                "ProfileView UI population failed. Trigger={Trigger}, UserId={UserId}, FullName={FullName}, PreferencesCount={PreferencesCount}.",
                trigger,
                _viewModel.ProfileDto.UserId,
                _viewModel.ProfileDto.FullName,
                _viewModel.Notifications.NotificationPreferences.Count);
            throw;
        }
    }

    private void PopulateUi()
    {
        ProfileDto user = _viewModel.ProfileDto;
        Log.Information(
            "ProfileView populating UI for UserId={UserId}, HasPhone={HasPhone}.",
            user.UserId,
            !string.IsNullOrWhiteSpace(user.PhoneNumber));
        ProfileCardName.Text = user.FullName ?? string.Empty;
        ProfileCardEmail.Text = user.Email ?? string.Empty;
        ProfileCardPhone.Text = user.PhoneNumber ?? string.Empty;
        ProfileCardAddress.Text = user.Address ?? string.Empty;
        FullNameBox.Text = user.FullName ?? string.Empty;
        EmailBox.Text = user.Email ?? string.Empty;
        PhoneBox.Text = user.PhoneNumber ?? string.Empty;
        AddressBox.Text = user.Address ?? string.Empty;
        PopulateNotificationPreferences(_viewModel.Notifications.NotificationPreferences);
    }

    private void SetEditingEnabled(bool enabled)
    {
        FullNameBox.IsEnabled = enabled;
        PhoneBox.IsEnabled = enabled;
        AddressBox.IsEnabled = enabled;
        SaveButton.IsEnabled = enabled;
        FullNameBox.IsReadOnly = !enabled;
        PhoneBox.IsReadOnly = !enabled;
        AddressBox.IsReadOnly = !enabled;
        FullNameBox.Opacity = enabled ? EnabledFormOpacity : DisabledFormOpacity;
        PhoneBox.Opacity = enabled ? EnabledFormOpacity : DisabledFormOpacity;
        AddressBox.Opacity = enabled ? EnabledFormOpacity : DisabledFormOpacity;
        
        UpdateButton.Content = enabled ? "Cancel Update" : "Unlock Update";

        if (enabled)
        {
            PhoneBox.Focus(FocusState.Programmatic);
            AddressBox.Focus(FocusState.Programmatic);
        }
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (SaveButton.IsEnabled)
        {
            // Cancel the update mode
            _verifiedPassword = string.Empty;
            SetEditingEnabled(false);
            TryPopulateUi("cancel-update");
            return;
        }

        _isChangingPasswordFlow = false;
        VerifyCurrentPasswordBox.Password = string.Empty;
        VerifyErrorInfoBar.IsOpen = false;
        try
        {
            await VerifyPasswordDialog.ShowAsync();
        }
        catch
        {
            // Dialog display failure is non-critical.
        }
    }

    private async void VerifyPasswordDialog_PrimaryButtonClick(
        ContentDialog sender,
        ContentDialogButtonClickEventArgs arguments)
    {
        ContentDialogButtonClickDeferral? deferral = arguments.GetDeferral();
        if (string.IsNullOrWhiteSpace(VerifyCurrentPasswordBox.Password))
        {
            VerifyErrorInfoBar.Message = "Enter your password.";
            VerifyErrorInfoBar.IsOpen = true;
            arguments.Cancel = true;
            deferral.Complete();
            return;
        }

        bool verified;
        try
        {
            verified = await _viewModel.PersonalInfo.VerifyPassword(VerifyCurrentPasswordBox.Password);
        }
        catch (Exception ex)
        {
            VerifyErrorInfoBar.Message = ex.Message;
            VerifyErrorInfoBar.IsOpen = true;
            arguments.Cancel = true;
            deferral.Complete();
            return;
        }

        if (!verified)
        {
            VerifyErrorInfoBar.Message = "Incorrect password.";
            VerifyErrorInfoBar.IsOpen = true;
            arguments.Cancel = true;
            deferral.Complete();
            return;
        }

        _verifiedPassword = VerifyCurrentPasswordBox.Password;
        VerifyErrorInfoBar.IsOpen = false;
        deferral.Complete();
        if (_isChangingPasswordFlow)
        {
            DispatcherQueue.TryEnqueue(async void () =>
            {
                try
                {
                    NewPasswordBox.Password = string.Empty;
                    ConfirmPasswordBox.Password = string.Empty;
                    NewPasswordErrorInfoBar.IsOpen = false;
                    await NewPasswordDialog.ShowAsync();
                }
                catch
                {
                    // Dialog display failure is non-critical.
                }
            });
        }
        else
        {
            SetEditingEnabled(true);
            ShowSuccess("You can now edit your profile.");
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ShowLoading(true);
        try
        {
            bool success = await _viewModel.PersonalInfo.UpdatePersonalInfo(
                PhoneBox.Text,
                AddressBox.Text,
                _verifiedPassword,
                FullNameBox.Text);
            ShowLoading(false);
            if (success)
            {
                ProfileCardName.Text = FullNameBox.Text.Trim();
                ProfileCardPhone.Text = PhoneBox.Text.Trim();
                ProfileCardAddress.Text = AddressBox.Text.Trim();
                _verifiedPassword = string.Empty;
                SetEditingEnabled(false);
                ShowSuccess("Profile updated successfully.");
            }
            else
            {
                ShowError("Failed to update profile.");
            }
        }
        catch (Exception ex)
        {
            ShowLoading(false);
            ShowError(ex.Message);
        }
    }

    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        _isChangingPasswordFlow = true;
        VerifyCurrentPasswordBox.Password = string.Empty;
        VerifyErrorInfoBar.IsOpen = false;
        try
        {
            await VerifyPasswordDialog.ShowAsync();
        }
        catch
        {
            // Dialog display failure is non-critical.
        }
    }

    private async void NewPasswordDialog_PrimaryButtonClick(
        ContentDialog sender,
        ContentDialogButtonClickEventArgs arguments)
    {
        ContentDialogButtonClickDeferral? deferral = arguments.GetDeferral();
        string? newPassword = NewPasswordBox.Password;
        string? confirmPassword = ConfirmPasswordBox.Password;
        int? userId = _viewModel.ProfileDto.UserId;
        if (userId == null)
        {
            NewPasswordErrorInfoBar.Message = "User not loaded.";
            NewPasswordErrorInfoBar.IsOpen = true;
            arguments.Cancel = true;
            deferral.Complete();
            return;
        }

        bool success;
        string errorMessage;
        try
        {
            (success, errorMessage) = await _viewModel.Security.ChangePassword(
                userId.Value,
                _verifiedPassword,
                newPassword,
                confirmPassword);
        }
        catch (Exception ex)
        {
            NewPasswordErrorInfoBar.Message = ex.Message;
            NewPasswordErrorInfoBar.IsOpen = true;
            arguments.Cancel = true;
            deferral.Complete();
            return;
        }

        if (success)
        {
            _verifiedPassword = string.Empty;
            NewPasswordErrorInfoBar.IsOpen = false;
            deferral.Complete();
            ShowSuccess("Your password has been changed successfully.");
        }
        else
        {
            NewPasswordErrorInfoBar.Message = errorMessage;
            NewPasswordErrorInfoBar.IsOpen = true;
            arguments.Cancel = true;
            deferral.Complete();
        }
    }

    private void DashboardNavButton_Click(object sender, RoutedEventArgs e) =>
        _navigationService.NavigateTo<DashboardView>();

    private void LogoutButton_Click(object sender, RoutedEventArgs e) =>
        _navigationService.NavigateTo<LoginView>();

    private void ShowLoading(bool visible)
    {
        LoadingPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        LoadingRing.IsActive = visible;
        ErrorInfoBar.IsOpen = false;
        SuccessInfoBar.IsOpen = false;
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
        SuccessInfoBar.IsOpen = false;
    }

    private void ShowSuccess(string message)
    {
        SuccessInfoBar.Message = message;
        SuccessInfoBar.IsOpen = true;
        ErrorInfoBar.IsOpen = false;
    }

    private void SwitchToTab(FrameworkElement activePanel, Button activeButton)
    {
        PanelPersonal.Visibility = Visibility.Collapsed;
        PanelSecurity.Visibility = Visibility.Collapsed;
        PanelNotifications.Visibility = Visibility.Collapsed;
        PanelSessions.Visibility = Visibility.Collapsed;
        TabPersonalBtn.Style = (Style)Resources["TabButtonStyle"];
        TabSecurityBtn.Style = (Style)Resources["TabButtonStyle"];
        TabNotificationsBtn.Style = (Style)Resources["TabButtonStyle"];
        TabSessionsBtn.Style = (Style)Resources["TabButtonStyle"];
        activePanel.Visibility = Visibility.Visible;
        activeButton.Style = (Style)Resources["TabButtonActiveStyle"];

        if (activePanel != PanelPersonal && SaveButton.IsEnabled)
        {
            _verifiedPassword = string.Empty;
            SetEditingEnabled(false);
            TryPopulateUi("tab-switch-cancel");
        }
    }

    private void TabPersonalBtn_Click(object sender, RoutedEventArgs e) =>
        SwitchToTab(PanelPersonal, TabPersonalBtn);

    private void TabSecurityBtn_Click(object sender, RoutedEventArgs e) =>
        SwitchToTab(PanelSecurity, TabSecurityBtn);

    private void TabNotificationsBtn_Click(object sender, RoutedEventArgs e) =>
        SwitchToTab(PanelNotifications, TabNotificationsBtn);

    private async void TabSessionsBtn_Click(object sender, RoutedEventArgs e)
    {
        SwitchToTab(PanelSessions, TabSessionsBtn);
        try
        {
            await LoadSessionsAsync();
        }
        catch
        {
            // LoadSessionsAsync surfaces errors through SessionsErrorBar.
        }
    }

    private Brush GetPrimaryTextBrush()
    {
        if (Resources.TryGetValue("TextPrimary", out object resource) && resource is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(
            ColorHelper.FromArgb(OpaqueColorAlpha, PrimaryTextRed, PrimaryTextGreen, PrimaryTextBlue));
    }
}
