namespace BankingApp.Desktop.Views;

using System;
using Contracts.Features.Billers.Dtos;
using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>
///     Hosts the multistep bill payment wizard.
/// </summary>
public sealed partial class BillPayView
{
    private const decimal ZeroAmount = 0m;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillPayView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model injected via DI.</param>
    public BillPayView(BillPayViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
        Loaded += OnPageLoaded;
    }

    /// <summary>
    ///     Gets the view model driving this page.
    /// </summary>
    /// <value>The bill pay view model.</value>
    public BillPayViewModel ViewModel { get; }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.LoadAsync();
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }

    private void SearchBox_TextChanged(
        AutoSuggestBox sender,
        AutoSuggestBoxTextChangedEventArgs autoSuggestBoxTextChangedEventArgs)
    {
        if (autoSuggestBoxTextChangedEventArgs.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.SearchCommand.Execute(null);
        }
    }

    private void CategoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    private void BillersList_ItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
    {
        if (itemClickEventArgs.ClickedItem is BillerDto biller)
        {
            ViewModel.SelectBillerCommand.Execute(biller);
        }
    }

    private void SavedBillersList_ItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
    {
        if (itemClickEventArgs.ClickedItem is SavedBillerDto savedBiller)
        {
            ViewModel.SelectBillerCommand.Execute(savedBiller);
        }
    }

    private void AmountBox_ValueChanged(
        NumberBox sender,
        NumberBoxValueChangedEventArgs numberBoxValueChangedEventArgs)
    {
        if (!double.IsNaN(sender.Value) && !double.IsInfinity(sender.Value))
        {
            ViewModel.Amount = Convert.ToDecimal(sender.Value);
        }
        else
        {
            ViewModel.Amount = ZeroAmount;
        }
    }
}
