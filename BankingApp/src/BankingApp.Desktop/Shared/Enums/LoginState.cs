namespace BankingApp.Desktop.Shared.Enums;

/// <summary>Represents the possible states of the login flow.</summary>
public enum LoginState
{
    /// <summary>No login attempt is in progress.</summary>
    Idle,
    /// <summary>A login request is in progress.</summary>
    Loading,
    /// <summary>Authentication succeeded.</summary>
    Success,
    /// <summary>The provided email or password did not match any account.</summary>
    InvalidCredentials,
    /// <summary>The account has been temporarily locked.</summary>
    AccountLocked,
    /// <summary>An unexpected error occurred during login.</summary>
    Error,
    /// <summary>The application is not properly configured and cannot connect to the server.</summary>
    ServerNotConfigured
}
