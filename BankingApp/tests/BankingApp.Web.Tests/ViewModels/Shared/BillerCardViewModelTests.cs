namespace BankingApp.Web.Tests.ViewModels.Shared;

using Contracts.Features.Billers.Dtos;
using BankingApp.Web.ViewModels.Shared;

public sealed class BillerCardViewModelTests
{
    [Fact]
    public void FromSavedBiller_MapsAllFields()
    {
        var biller = new SavedBillerDto()
        {
            BillerId         = 7,
            BillerName       = "Electric Co.",
            BillerCategory   = "Utilities",
            LogoUrl          = "https://example.com/logo.png",
            DefaultReference = "INV-999",
            Nickname         = null
        };

        var result = BillerCardViewModel.FromSavedBiller(biller);

        result.BillerId.Should().Be(7);
        result.DisplayName.Should().Be("Electric Co.");
        result.Category.Should().Be("Utilities");
        result.LogoUrl.Should().Be("https://example.com/logo.png");
        result.DefaultReference.Should().Be("INV-999");
    }

    [Fact]
    public void FromSavedBiller_WhenNicknameIsSet_UsesNicknameAsDisplayName()
    {
        var biller = new SavedBillerDto()
        {
            BillerName = "Electric Co.",
            Nickname   = "My Electric"
        };

        var result = BillerCardViewModel.FromSavedBiller(biller);

        result.DisplayName.Should().Be("My Electric");
    }

    [Fact]
    public void FromSavedBiller_WhenBillerHasCategory_UsesBillerCategory()
    {
        var billerDetail = new BillerDto { Category = "Utilities Premium" };
        var biller = new SavedBillerDto()
        {
            BillerCategory = "Utilities",
            Biller         = billerDetail
        };

        var result = BillerCardViewModel.FromSavedBiller(biller);

        result.Category.Should().Be("Utilities Premium");
    }
}
