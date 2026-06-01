namespace BankingApp.Web.ViewModels.Shared;

using Contracts.Features.Billers.Dtos;

public sealed class BillerCardViewModel
{
    public int BillerId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Category { get; init; }
    public string? LogoUrl { get; init; }
    public string? DefaultReference { get; init; }

    public static BillerCardViewModel FromSavedBiller(SavedBillerDto biller) =>
        new()
        {
            BillerId         = biller.BillerId,
            DisplayName      = biller.DisplayName,
            Category         = biller.DisplayCategory,
            LogoUrl          = biller.LogoUrl,
            DefaultReference = biller.DefaultReference
        };
}
