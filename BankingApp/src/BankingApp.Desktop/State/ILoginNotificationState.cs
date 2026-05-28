namespace BankingApp.Desktop.State;

/// <summary>Stores short-lived notifications that should be shown on the login screen.</summary>
public interface ILoginNotificationState
{
    /// <summary>Gets or sets a value indicating whether the login screen should show the registration success message.</summary>
    public bool ShowRegistrationSuccess { get; set; }
}
