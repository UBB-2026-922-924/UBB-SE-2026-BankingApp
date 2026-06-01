namespace BankingApp.Desktop.ViewModels;

public partial class LoansAndSavingsViewModel : ObservableObject, IDisposable
{
    public SavingsViewModel SavingsVm { get; }

    public LoansViewModel LoansVm { get; }

    public LoansAndSavingsViewModel(SavingsViewModel savingsVm, LoansViewModel loansVm)
    {
        SavingsVm = savingsVm;
        LoansVm = loansVm;
    }

    public void Dispose()
    {
        SavingsVm.Dispose();
    }
}
