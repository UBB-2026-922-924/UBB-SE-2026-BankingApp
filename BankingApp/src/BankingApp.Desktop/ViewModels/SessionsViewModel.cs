namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using Shared.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>Handles active-session loading and revocation for the profile area.</summary>
public partial class SessionsViewModel : ObservableObject
{
    private readonly IProfileService _profileService;
    private readonly ILogger<SessionsViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="SessionsViewModel"/> class.</summary>
    public SessionsViewModel(IProfileService profileService, ILogger<SessionsViewModel> logger)
    {
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ActiveSessions = new List<SessionDto>();
    }

    /// <summary>Gets or sets the current sessions workflow state.</summary>
    [ObservableProperty]
    public partial ProfileState State { get; set; } = ProfileState.Idle;

    /// <summary>Gets the active sessions currently loaded for the user.</summary>
    public List<SessionDto> ActiveSessions { get; private set; }

    /// <summary>Loads active sessions for the current authenticated user.</summary>
    public async Task<bool> LoadSessionsAsync(int userId)
    {
        State = ProfileState.Loading;
        try
        {
            ErrorOr<List<SessionDto>> result = await _profileService.GetSessionsAsync();
            if (result.IsError)
            {
                ActiveSessions = new List<SessionDto>();
                State = ProfileState.Error;
                return false;
            }

            ActiveSessions = result.Value;
            State = ProfileState.Idle;
            return true;
        }
        catch (Exception exception)
        {
            DesktopLogMessages.LoadSessionsFailed(_logger, exception, userId);
            ActiveSessions = new List<SessionDto>();
            State = ProfileState.Error;
            return false;
        }
    }

    /// <summary>Revokes the specified active session.</summary>
    public async Task<bool> RevokeSessionAsync(int sessionId)
    {
        State = ProfileState.Loading;
        try
        {
            ErrorOr<Success> result = await _profileService.RevokeSessionAsync(sessionId);
            State = result.IsError ? ProfileState.Error : ProfileState.Idle;
            return !result.IsError;
        }
        catch (Exception exception)
        {
            DesktopLogMessages.RevokeSessionFailed(_logger, exception, sessionId);
            State = ProfileState.Error;
            return false;
        }
    }
}