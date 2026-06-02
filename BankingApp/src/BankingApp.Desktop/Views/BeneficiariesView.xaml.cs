namespace BankingApp.Desktop.Views;

using Contracts.Features.Beneficiaries.Dtos;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViewModels;

/// <summary>
///     Page that displays beneficiaries and CRUD interactions for the desktop app.
/// </summary>
public sealed partial class BeneficiariesView
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BeneficiariesView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model instance to bind to the page.</param>
    public BeneficiariesView(BeneficiariesViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
        Loaded += OnPageLoaded;
    }

    /// <summary>
    ///     Gets the view model backing this page.
    /// </summary>
    public BeneficiariesViewModel ViewModel { get; }

    private async void OnPageLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        try
        {
            await ViewModel.LoadBeneficiariesAsync();
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }

    private void ShowAddForm_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        ViewModel.StartAddBeneficiary();
    }

    private void CancelAdd_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        ViewModel.CancelEditing();
    }

    private async void Save_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        try
        {
            await ViewModel.SaveBeneficiaryAsync();
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        BeneficiaryDto? beneficiary = TryGetBeneficiaryFromSender(sender);
        if (beneficiary is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Delete beneficiary",
            Content = $"Remove {beneficiary.Name} from your beneficiaries?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        ContentDialogResult dialogResult = await dialog.ShowAsync();
        if (dialogResult != ContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            await ViewModel.DeleteBeneficiaryAsync(beneficiary.Id);
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        BeneficiaryDto? beneficiary = TryGetBeneficiaryFromSender(sender);
        if (beneficiary is null)
        {
            return;
        }

        ViewModel.StartEditBeneficiary(beneficiary);
    }

    private void Use_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        BeneficiaryDto? beneficiary = TryGetBeneficiaryFromSender(sender);
        if (beneficiary is null)
        {
            return;
        }

        ViewModel.UseForTransfer(beneficiary);
    }

    private static BeneficiaryDto? TryGetBeneficiaryFromSender(object sender)
    {
        return sender is Button { Tag: BeneficiaryDto beneficiary } ? beneficiary : null;
    }
}
