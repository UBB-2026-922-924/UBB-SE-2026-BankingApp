namespace BankingApp.Desktop.Views;

using System;
using System.Linq;
using ErrorOr;
using Shared.Enums;
using ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Navigation;
using State;

/// <summary>
///     Displays the login form and reacts to authentication state changes produced by <see cref="LoginViewModel" />.
/// </summary>
public sealed partial class LoginView
{
    private readonly IAppNavigationService _navigationService;
    private readonly LoginViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives authentication logic and exposes login state.</param>
    /// <param name="navigationService">Used to navigate to other pages in response to state changes.</param>
    /// <param name="loginNotificationState">Carries one-shot notifications to display on the login page.</param>
    /// <param name="configuration">Application configuration used to display the active API endpoint.</param>
    /// <returns>The result of the operation.</returns>
    public LoginView(
        LoginViewModel viewModel,
        IAppNavigationService navigationService,
        ILoginNotificationState loginNotificationState,
        IConfiguration configuration)
    {
        _navigationService = navigationService;
        InitializeComponent();
        _viewModel = viewModel;
        ServerConnectionText.Text = BuildServerConnectionText(configuration["ApiBaseUrl"]);
        DevLoginButton.Visibility = _viewModel.IsDevLoginAvailable
            ? Visibility.Visible
            : Visibility.Collapsed;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        if (loginNotificationState.ShowRegistrationSuccess)
        {
            loginNotificationState.ShowRegistrationSuccess = false;
            RegistrationSuccessBar.IsOpen = true;
        }

        if (_viewModel.SavedRememberMe && !string.IsNullOrWhiteSpace(_viewModel.SavedEmail))
        {
            EmailBox.Text = _viewModel.SavedEmail;
            RememberMeCheckBox.IsChecked = true;
        }

        // Apply the ViewModel's current state immediately. The ViewModel is constructed
        // before the view subscribes, so any state set in the constructor (e.g.
        // ServerNotConfigured when ApiBaseUrl is missing) would otherwise be missed.
        OnStateChanged(_viewModel.State);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LoginViewModel.State))
        {
            OnStateChanged(_viewModel.State);
        }
    }

    private void OnStateChanged(LoginState state)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            HideLoading();
            ErrorInfoBar.IsOpen = false;
            switch (state)
            {
                case LoginState.Idle:
                    EnableForm();
                    break;
                case LoginState.Loading:
                    ShowLoading();
                    break;
                case LoginState.Success:
                    EnableForm();
                    _navigationService.NavigateTo<NavigationView>();
                    break;
                case LoginState.InvalidCredentials:
                    EnableForm();
                    ShowError("Invalid email or password.");
                    break;
                case LoginState.AccountLocked:
                    EnableForm();
                    ShowError("Account is locked. Try again later.");
                    break;
                case LoginState.Error:
                    EnableForm();
                    ShowError("Something went wrong. Please try again.");
                    break;
                case LoginState.ServerNotConfigured:
                    // Form stays disabled — misconfiguration cannot be resolved at runtime.
                    ShowError("The application is not properly set up. Please contact your administrator.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        });
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
    }

    private void EnableForm()
    {
        SignInButton.IsEnabled = true;
    }

    private void ShowLoading()
    {
        LoadingRing.IsActive = true;
        LoadingRing.Visibility = Visibility.Visible;
        SignInButton.IsEnabled = false;
        DevLoginButton.IsEnabled = false;
    }

    private void HideLoading()
    {
        LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;
        DevLoginButton.IsEnabled = true;
    }

    private async void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        string? email = EmailBox.Text;
        string? password = PasswordBox.Password;
        if (!LoginViewModel.CanLogin(email, password))
        {
            ShowError("Please enter email and password.");
            return;
        }

        bool rememberMe = RememberMeCheckBox.IsChecked == true;

        try
        {
            await _viewModel.Login(email, password, rememberMe);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<RegisterView>();
    }

    private async void DevLoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ErrorOr<Success> result = await _viewModel.DevLogin();
            result.Switch(
                _ => { },
                errors => ShowError(errors.First().Description));
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private static string BuildServerConnectionText(string? apiBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            return "Connected: not configured";
        }

        if (Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out Uri? uri))
        {
            return $"Connected: {uri.Authority}";
        }

        return $"Connected: {apiBaseUrl}";
    }
}
