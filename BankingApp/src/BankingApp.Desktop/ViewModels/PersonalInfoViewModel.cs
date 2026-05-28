namespace BankingApp.Desktop.ViewModels;

using System;
using System.Threading.Tasks;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using Shared.Enums;
using ErrorOr;
using Logging;
using Microsoft.Extensions.Logging;
using Shared;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>Handles personal-profile loading, editing, and password verification for the profile area.</summary>
public partial class PersonalInfoViewModel : ObservableObject
{
    private readonly IProfileService _profileService;
    private readonly ILogger<PersonalInfoViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="PersonalInfoViewModel"/> class.</summary>
    public PersonalInfoViewModel(IProfileService profileService, ILogger<PersonalInfoViewModel> logger)
    {
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ProfileInfo = new ProfileDto();
    }

    /// <summary>Gets or sets the current profile workflow state.</summary>
    [ObservableProperty]
    public partial ProfileState State { get; set; } = ProfileState.Idle;

    /// <summary>Gets the loaded profile details.</summary>
    public ProfileDto ProfileInfo { get; private set; }

    /// <summary>Gets the loaded profile details using the legacy property name expected elsewhere in the UI.</summary>
    public ProfileDto ProfileDto => ProfileInfo;

    /// <summary>Gets a value indicating whether the user has a phone number on file.</summary>
    public bool HasPhoneNumber => !string.IsNullOrEmpty(ProfileInfo.PhoneNumber);

    /// <summary>Gets the phone-number text shown in the two-factor section.</summary>
    public string TwoFactorPhoneDisplay =>
        HasPhoneNumber ? ProfileInfo.PhoneNumber! : UserMessages.Profile.NoPhoneNumber;

    /// <summary>Loads the current user's profile.</summary>
    public async Task<bool> LoadProfile()
    {
        State = ProfileState.Loading;
        ErrorOr<ProfileDto> profileResult = await _profileService.GetProfileAsync();
        if (profileResult.IsError)
        {
            DesktopLogMessages.LoadProfileFailed(_logger, profileResult.Errors);
            State = ProfileState.Error;
            return false;
        }

        ProfileInfo = profileResult.Value;
        State = ProfileState.UpdateSuccess;
        return true;
    }

    /// <summary>Updates editable personal-information fields for the current user.</summary>
    public async Task<bool> UpdatePersonalInfo(string? phone, string? address, string password, string? fullName = null)
    {
        State = ProfileState.Loading;
        if (ProfileInfo.UserId == null)
        {
            State = ProfileState.Error;
            return false;
        }

        string? trimmedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        string? trimmedAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        string? trimmedFullName = string.IsNullOrWhiteSpace(fullName) ? ProfileInfo.FullName : fullName.Trim();
        var request = new UpdateProfileRequest
        {
            UserId = ProfileInfo.UserId,
            FullName = trimmedFullName,
            PhoneNumber = trimmedPhone,
            DateOfBirth = ProfileInfo.DateOfBirth,
            Address = trimmedAddress,
            Nationality = ProfileInfo.Nationality,
            PreferredLanguage = ProfileInfo.PreferredLanguage,
        };
        ErrorOr<Success> result = await _profileService.UpdateProfileAsync(request);
        return result.Match(
            _ =>
            {
                ProfileInfo.FullName = trimmedFullName;
                ProfileInfo.PhoneNumber = trimmedPhone;
                ProfileInfo.Address = trimmedAddress;
                State = ProfileState.UpdateSuccess;
                return true;
            },
            errors =>
            {
                DesktopLogMessages.UpdatePersonalInfoFailed(_logger, errors);
                State = ProfileState.Error;
                return false;
            });
    }

    /// <summary>Verifies the current password against the server.</summary>
    public async Task<bool> VerifyPassword(string password)
    {
        State = ProfileState.Loading;
        if (ProfileInfo.UserId == null)
        {
            State = ProfileState.Error;
            return false;
        }

        ErrorOr<bool> result = await _profileService.VerifyPasswordAsync(password);
        return result.Match(
            valid =>
            {
                if (!valid)
                {
                    State = ProfileState.Error;
                    return false;
                }

                State = ProfileState.UpdateSuccess;
                return true;
            },
            errors =>
            {
                DesktopLogMessages.VerifyPasswordFailed(_logger, errors);
                State = ProfileState.Error;
                return false;
            });
    }
}