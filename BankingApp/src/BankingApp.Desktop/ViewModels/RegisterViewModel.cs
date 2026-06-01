namespace BankingApp.Desktop.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Authentication.Services;
using Contracts.Features.UserRegistration.Dtos;
using Shared.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Shared.Validation;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>Coordinates user-registration requests for the register view.</summary>
public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<RegisterViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="RegisterViewModel"/> class.</summary>
    public RegisterViewModel(IAuthenticationService authenticationService, ILogger<RegisterViewModel> logger)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Gets or sets the current registration workflow state.</summary>
    [ObservableProperty]
    public partial RegisterState State { get; set; } = RegisterState.Idle;

    /// <summary>Registers a new account using the supplied email, password, and full name.</summary>
    public async Task Register(string email, string password, string confirmPassword, string fullName)
    {
        email = email.Trim();
        fullName = fullName.Trim();
        RegisterState? validationError = ValidateLocally(email, password, confirmPassword, fullName);
        if (validationError != null)
        {
            State = validationError.Value;
            return;
        }

        State = RegisterState.Loading;
        ErrorOr<Success> result = await _authenticationService.RegisterAsync(
            new RegisterRequest { Email = email, Password = password, FullName = fullName });
        result.Switch(
            _ => State = RegisterState.Success,
            errors =>
            {
                Error error = errors.First();
                if (error.Type == ErrorType.Conflict)
                {
                    State = RegisterState.EmailAlreadyExists;
                }
                else if (error.Code == "invalid_email")
                {
                    State = RegisterState.InvalidEmail;
                }
                else if (error.Code == "weak_password")
                {
                    State = RegisterState.WeakPassword;
                }
                else
                {
                    DesktopLogMessages.RegisterFailed(_logger, errors);
                    State = RegisterState.Error;
                }
            });
    }

    private static RegisterState? ValidateLocally(string email, string password, string confirmPassword,
        string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(confirmPassword))
        {
            return RegisterState.Error;
        }

        if (!email.Contains('@', StringComparison.Ordinal))
        {
            return RegisterState.InvalidEmail;
        }

        if (!PasswordValidator.IsStrong(password))
        {
            return RegisterState.WeakPassword;
        }

        if (password != confirmPassword)
        {
            return RegisterState.PasswordMismatch;
        }

        return null;
    }
}
