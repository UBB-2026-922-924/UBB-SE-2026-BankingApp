namespace BankingApp.Desktop.Shared.Enums;

/// <summary>Represents the possible states of the profile management flow.</summary>
public enum ProfileState
{
    /// <summary>No operation is in progress.</summary>
    Idle,
    /// <summary>A profile operation is in progress.</summary>
    Loading,
    /// <summary>Profile data was loaded successfully.</summary>
    Success,
    /// <summary>A profile field update completed successfully.</summary>
    UpdateSuccess,
    /// <summary>A password change completed successfully.</summary>
    PasswordChanged,
    /// <summary>An unexpected error occurred during a profile operation.</summary>
    Error
}
