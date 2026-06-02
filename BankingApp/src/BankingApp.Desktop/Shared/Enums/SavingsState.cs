namespace BankingApp.Desktop.Shared.Enums;

/// <summary>
///     Describes the loading state of the savings workspace.
/// </summary>
public enum SavingsState
{
    /// <summary>No operation is currently running.</summary>
    Idle,
    /// <summary>Savings data is being loaded or submitted.</summary>
    Loading,
    /// <summary>Savings data is ready to display.</summary>
    Ready,
    /// <summary>The savings workflow encountered an error.</summary>
    Error
}
