namespace BankingApp.Desktop.Shared.Enums;

/// <summary>
///     Describes the loading state of the investments workspace.
/// </summary>
public enum InvestmentsState
{
    /// <summary>No operation is currently running.</summary>
    Idle,
    /// <summary>Investment data is being loaded or submitted.</summary>
    Loading,
    /// <summary>Investment data is ready to display.</summary>
    Ready,
    /// <summary>The investments workflow encountered an error.</summary>
    Error
}
