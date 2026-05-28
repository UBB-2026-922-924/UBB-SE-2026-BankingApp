namespace BankingApp.Desktop.State;

/// <inheritdoc />
public sealed class LoginNotificationState : ILoginNotificationState
{
    /// <inheritdoc />
    public bool ShowRegistrationSuccess { get; set; }
}
