namespace BankingApp.Desktop.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using Shared.Enums;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using ErrorOr;
using Logging;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Validation;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>Handles password changes for the profile area.</summary>
public partial class SecurityViewModel : ObservableObject
{
    private readonly IProfileService _profileService;
    private readonly ILogger<SecurityViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="SecurityViewModel"/> class.</summary>
    public SecurityViewModel(IProfileService profileService, ILogger<SecurityViewModel> logger)
    {
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Gets or sets the current security workflow state.</summary>
    [ObservableProperty]
    public partial ProfileState State { get; set; } = ProfileState.Idle;

    /// <summary>Changes the user's password.</summary>
    public async Task<(bool Success, string ErrorMessage)> ChangePassword(
        int userId,
        string currentPassword,
        string newPassword,
        string confirmPassword)
    {
        if (!PasswordValidator.MeetsMinimumLength(newPassword))
        {
            return (false, UserMessages.Security.MinimumLengthRequired);
        }

        if (newPassword != confirmPassword)
        {
            return (false, UserMessages.Security.PasswordMismatch);
        }

        State = ProfileState.Loading;
        var request = new ChangePasswordRequest(userId, currentPassword, newPassword);
        ErrorOr<Success> result = await _profileService.ChangePasswordAsync(request);
        return result.Match(
            _ =>
            {
                State = ProfileState.UpdateSuccess;
                return (true, string.Empty);
            },
            errors =>
            {
                DesktopLogMessages.ChangePasswordFailed(_logger, errors);
                State = ProfileState.Error;
                string message = errors.First().Code == "incorrect_password"
                    ? UserMessages.Security.IncorrectPassword
                    : UserMessages.Security.UnexpectedError;
                return (false, message);
            });
    }
}
