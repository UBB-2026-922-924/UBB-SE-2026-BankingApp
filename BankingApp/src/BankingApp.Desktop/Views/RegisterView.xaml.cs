namespace BankingApp.Desktop.Views;

using ViewModels;
using Microsoft.UI.Xaml;
using Navigation;
using Shared;
using Shared.Enums;
using State;

/// <summary>
///     Displays the registration form and reacts to registration state changes.
/// </summary>
public sealed partial class RegisterView
{
    private readonly IAppNavigationService _navigationService;
    private readonly ILoginNotificationState _loginNotificationState;
    private readonly RegisterViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegisterView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives registration logic and exposes registration state.</param>
    /// <param name="navigationService">Used to navigate to other pages in response to state changes.</param>
    /// <param name="loginNotificationState">Carries the registration success notification to the login page.</param>
    /// <returns>The result of the operation.</returns>
    public RegisterView(
        RegisterViewModel viewModel,
        IAppNavigationService navigationService,
        ILoginNotificationState loginNotificationState)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navigationService = navigationService;
        _loginNotificationState = loginNotificationState;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        OnStateChanged(_viewModel.State);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RegisterViewModel.State))
        {
            OnStateChanged(_viewModel.State);
        }
    }

    private void OnStateChanged(RegisterState state)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            HideLoading();
            ErrorInfoBar.IsOpen = false;
            switch (state)
            {
                case RegisterState.Idle:
                    break;
                case RegisterState.Loading:
                    ShowLoading();
                    break;
                case RegisterState.Success:
                    _loginNotificationState.ShowRegistrationSuccess = true;
                    _navigationService.NavigateTo<LoginView>();
                    break;
                case RegisterState.AutoLoggedIn:
                    _navigationService.NavigateTo<NavigationView>();
                    break;
                case RegisterState.EmailAlreadyExists:
                    ShowError(UserMessages.Register.EmailAlreadyExists);
                    break;
                case RegisterState.InvalidEmail:
                    ShowError(UserMessages.Register.InvalidEmail);
                    break;
                case RegisterState.WeakPassword:
                    ShowError(UserMessages.Register.WeakPassword);
                    break;
                case RegisterState.PasswordMismatch:
                    ShowError(UserMessages.Register.PasswordMismatch);
                    break;
                case RegisterState.Error:
                    ShowError(UserMessages.Register.AllFieldsRequired);
                    break;
            }
        });
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
    }

    private void ShowLoading()
    {
        LoadingRing.IsActive = true;
        LoadingRing.Visibility = Visibility.Visible;
        RegisterButton.IsEnabled = false;
    }

    private void HideLoading()
    {
        LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;
        RegisterButton.IsEnabled = true;
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.Register(
            EmailBox.Text,
            PasswordBox.Password,
            ConfirmPasswordBox.Password,
            FullNameBox.Text);
    }

    private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<LoginView>();
    }
}
