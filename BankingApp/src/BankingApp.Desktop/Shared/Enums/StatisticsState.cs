namespace BankingApp.Desktop.Shared.Enums;

/// <summary>
///     Describes the loading state of the statistics workspace.
/// </summary>
public enum StatisticsState
{
    /// <summary>No operation is currently running.</summary>
    Idle,
    /// <summary>Statistics data is being loaded.</summary>
    Loading,
    /// <summary>Statistics data is ready to display.</summary>
    Ready,
    /// <summary>The statistics workflow encountered an error.</summary>
    Error
}
