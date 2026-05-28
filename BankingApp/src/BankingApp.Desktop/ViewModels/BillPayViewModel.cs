namespace BankingApp.Desktop.ViewModels;

using System.Collections.ObjectModel;
using System.Windows.Input;
using BankingApp.Contracts.Features.BillPayments.Dtos;
using BankingApp.Contracts.Features.Billers.Dtos;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.Billers.Services;
using Microsoft.UI.Xaml;
using Navigation;
using Views;

/// <summary>Coordinates the multistep bill payment workflow in the desktop client.</summary>
public partial class BillPayViewModel : ObservableObject
{
    private const int SelectBillerStep = 1;
    private const int PaymentDetailsStep = 2;
    private const int ReviewAndConfirmStep = 3;
    private const int PaymentResultStep = 4;
    private const int MinimumBillers = 0;
    private const int MinimumAmount = 0;
    private const int NoFee = 0;

    private readonly IBillPaymentService _billPaymentService;
    private readonly IBillerService _billerService;
    private readonly IAppNavigationService _navigationService;

    /// <summary>Initializes a new instance of the <see cref="BillPayViewModel"/> class.</summary>
    public BillPayViewModel(
        IBillPaymentService billPaymentService,
        IBillerService billerService,
        IAppNavigationService navigationService)
    {
        _billPaymentService = billPaymentService;
        _billerService = billerService;
        _navigationService = navigationService;

        Billers = new ObservableCollection<BillerDto>();
        SavedBillers = new ObservableCollection<SavedBillerDto>();
        Accounts = new ObservableCollection<AccountDto>();
        CurrentStep = SelectBillerStep;

        SearchCommand = new RelayCommand(ExecuteSearch);
        SelectBillerCommand = new RelayCommand<object?>(ExecuteSelectBiller);
        NextStepCommand = new AsyncRelayCommand(ExecuteNextStepAsync);
        BackCommand = new RelayCommand(ExecuteBack);
        PayAnotherBillCommand = new RelayCommand(ResetForm);
        PayBillCommand = new AsyncRelayCommand(ExecutePayBillAsync);
        CancelCommand = new RelayCommand(ResetForm);
    }

    /// <summary>Gets the command that refreshes billers using the current filters.</summary>
    public ICommand SearchCommand { get; }

    /// <summary>Gets the command that selects a biller or saved biller.</summary>
    public ICommand SelectBillerCommand { get; }

    /// <summary>Gets the command that advances the payment wizard.</summary>
    public ICommand NextStepCommand { get; }

    /// <summary>Gets the command that returns to the previous wizard step.</summary>
    public ICommand BackCommand { get; }

    /// <summary>Gets the command that resets the flow after a completed payment.</summary>
    public ICommand PayAnotherBillCommand { get; }

    /// <summary>Gets the command that submits the current payment.</summary>
    public ICommand PayBillCommand { get; }

    /// <summary>Gets the command that abandons the bill payment flow.</summary>
    public ICommand CancelCommand { get; }

    /// <summary>Gets or sets the current wizard step.</summary>
    [ObservableProperty]
    public partial int CurrentStep { get; set; } = default!;

    /// <summary>Gets or sets the available billers.</summary>
    [ObservableProperty]
    public partial ObservableCollection<BillerDto> Billers { get; set; }

    /// <summary>Gets or sets the saved billers for the current user.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSavedBillers))]
    [NotifyPropertyChangedFor(nameof(SavedBillersVisibility))]
    public partial ObservableCollection<SavedBillerDto> SavedBillers { get; set; }

    /// <summary>Gets or sets the source accounts available for payment.</summary>
    [ObservableProperty]
    public partial ObservableCollection<AccountDto> Accounts { get; set; }

    /// <summary>Gets or sets the currently selected biller.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedBillerName))]
    public partial BillerDto? SelectedBiller { get; set; } = null!;

    partial void OnSelectedBillerChanged(BillerDto? value)
    {
        ApplySavedDefaultsForSelectedBiller();
    }

    /// <summary>Gets or sets the biller search text.</summary>
    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    /// <summary>Gets or sets the biller category filter.</summary>
    [ObservableProperty]
    public partial string? SelectedCategory { get; set; } = default!;

    partial void OnSelectedCategoryChanged(string? value)
    {
        ExecuteSearch();
    }

    /// <summary>Gets or sets the customer reference used by the selected biller.</summary>
    [ObservableProperty]
    public partial string BillerReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the bill amount.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReviewAmountText))]
    [NotifyPropertyChangedFor(nameof(Total))]
    [NotifyPropertyChangedFor(nameof(TotalText))]
    [NotifyPropertyChangedFor(nameof(AmountAsDouble))]
    public partial decimal Amount { get; set; } = default!;

    /// <summary>Gets or sets the bill amount as a <see cref="double"/> for XAML bindings.</summary>
    public double AmountAsDouble
    {
        get => (double)Amount;
        set => Amount = (decimal)value;
    }

    /// <summary>Gets or sets a value indicating whether the payment should settle the full balance.</summary>
    [ObservableProperty]
    public partial bool IsPayInFull { get; set; } = default!;

    /// <summary>Gets or sets the account used to fund the payment.</summary>
    [ObservableProperty]
    public partial AccountDto? SelectedAccount { get; set; } = default!;

    /// <summary>Gets or sets the calculated payment fee.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReviewFeeText))]
    [NotifyPropertyChangedFor(nameof(Total))]
    [NotifyPropertyChangedFor(nameof(TotalText))]
    public partial decimal Fee { get; set; } = default!;

    /// <summary>Gets or sets the receipt number returned after a successful payment.</summary>
    [ObservableProperty]
    public partial string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the current user-facing error message.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessageVisibility))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets the visibility of the error message panel.</summary>
    public Visibility ErrorMessageVisibility =>
        string.IsNullOrWhiteSpace(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Gets or sets a value indicating whether the selected biller should be saved for reuse.</summary>
    [ObservableProperty]
    public partial bool ShouldSaveBiller { get; set; } = default!;

    /// <summary>Gets a value indicating whether any saved billers are available.</summary>
    public bool HasSavedBillers => SavedBillers.Count > MinimumBillers;

    /// <summary>Gets the visibility of the saved billers section.</summary>
    public Visibility SavedBillersVisibility =>
        HasSavedBillers ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Gets the selected biller display name.</summary>
    public string SelectedBillerName => SelectedBiller?.Name ?? "No biller selected";

    /// <summary>Gets the formatted amount shown on the review step.</summary>
    public string ReviewAmountText =>
        Amount > MinimumAmount ? $"{Amount:0.00} RON" : "No amount entered";

    /// <summary>Gets the formatted fee shown on the review step.</summary>
    public string ReviewFeeText => $"{Fee:0.00} RON";

    /// <summary>Gets the payment total including fees.</summary>
    public decimal Total => Amount + Fee;

    /// <summary>Gets the formatted payment total shown on the review step.</summary>
    public string TotalText => $"{Total:0.00} RON";
}
