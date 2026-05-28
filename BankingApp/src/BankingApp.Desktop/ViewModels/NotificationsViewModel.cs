namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using Shared.Enums;
using ErrorOr;
using Logging;
using Microsoft.Extensions.Logging;
using Shared;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>Handles notification-preference loading and updates for the profile area.</summary>
public partial class NotificationsViewModel : ObservableObject
{
    private readonly IProfileService _profileService;
    private readonly ILogger<NotificationsViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="NotificationsViewModel"/> class.</summary>
    public NotificationsViewModel(IProfileService profileService, ILogger<NotificationsViewModel> logger)
    {
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        NotificationPreferences = [];
    }

    /// <summary>Gets or sets the current notifications workflow state.</summary>
    [ObservableProperty]
    public partial ProfileState State { get; set; } = ProfileState.Idle;

    /// <summary>Gets the current notification preferences.</summary>
    public List<NotificationPreferenceDto> NotificationPreferences { get; private set; }

    /// <summary>Toggles a single notification preference and rolls the change back if persistence fails.</summary>
    public async Task<bool> ToggleNotificationPreference(NotificationPreferenceDto preference, bool enabled)
    {
        bool previousValue = preference.EmailEnabled;
        preference.EmailEnabled = enabled;
        bool success = await UpdateNotificationPreferences(NotificationPreferences);
        if (!success)
        {
            preference.EmailEnabled = previousValue;
        }

        return success;
    }

    /// <summary>Loads notification preferences for the current user.</summary>
    public async Task<bool> LoadNotificationPreferences()
    {
        ErrorOr<List<NotificationPreferenceDto>> preferencesResult =
            await _profileService.GetNotificationPreferencesAsync();
        if (preferencesResult.IsError)
        {
            DesktopLogMessages.LoadNotificationPreferencesFailed(_logger, preferencesResult.Errors);
            return false;
        }

        NotificationPreferences = preferencesResult.Value;
        return true;
    }

    /// <summary>Persists the provided notification preferences.</summary>
    public async Task<bool> UpdateNotificationPreferences(List<NotificationPreferenceDto> preferences)
    {
        if (preferences.Count == 0)
        {
            return false;
        }

        State = ProfileState.Loading;
        ErrorOr<Success> result = await _profileService.UpdateNotificationPreferencesAsync(preferences);
        return result.Match(
            _ =>
            {
                NotificationPreferences = preferences;
                State = ProfileState.UpdateSuccess;
                return true;
            },
            errors =>
            {
                DesktopLogMessages.UpdateNotificationPreferencesFailed(_logger, errors);
                State = ProfileState.Error;
                return false;
            });
    }
}