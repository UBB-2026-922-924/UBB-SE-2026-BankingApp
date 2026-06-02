namespace BankingApp.Desktop.Shared.Enums;

/// <summary>
///     Describes the loading state of the loans workspace.
/// </summary>
public enum LoansState
{
    /// <summary>No operation is currently running.</summary>
    Idle,
    /// <summary>Loan data is being loaded or submitted.</summary>
    Loading,
    /// <summary>Loan data is ready to display.</summary>
    Ready,
    /// <summary>The loans workflow encountered an error.</summary>
    Error
}
