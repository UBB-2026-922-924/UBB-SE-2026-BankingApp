namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Coordinates the shared loans and savings workspace.
/// </summary>
public partial class LoansAndSavingsViewModel : ObservableObject, IDisposable
{
    /// <summary>
    ///     Gets the savings child view model.
    /// </summary>
    public SavingsViewModel SavingsVm { get; }

    /// <summary>
    ///     Gets the loans child view model.
    /// </summary>
    public LoansViewModel LoansVm { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoansAndSavingsViewModel" /> class.
    /// </summary>
    /// <param name="savingsVm">The savings child view model.</param>
    /// <param name="loansVm">The loans child view model.</param>
    public LoansAndSavingsViewModel(SavingsViewModel savingsVm, LoansViewModel loansVm)
    {
        SavingsVm = savingsVm;
        LoansVm = loansVm;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        SavingsVm.Dispose();
        GC.SuppressFinalize(this);
    }
}
