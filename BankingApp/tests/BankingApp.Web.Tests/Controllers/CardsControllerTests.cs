namespace BankingApp.Web.Tests.Controllers;

using BankingApp.Contracts.Features.Cards.Dtos;
using BankingApp.Contracts.Features.Cards.Services;
using BankingApp.Domain.Enums;
using BankingApp.Web.Controllers;
using BankingApp.Web.Models.Cards;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class CardsControllerTests : IDisposable
{
    private const int ExistingCardId = 42;

    private readonly Mock<ICardService> _cardServiceMock = new(MockBehavior.Strict);
    private readonly CardsController _controller;

    public CardsControllerTests()
    {
        DefaultHttpContext httpContext = new();

        _controller = new CardsController(_cardServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }

    public void Dispose() => _controller.Dispose();

    // -------------------------------------------------------------------------
    // Index
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Index_WhenServiceReturnsCards_ShouldReturnViewWithCards()
    {
        // Arrange
        DateTime expiryDate = new(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        List<CardDetailsDto> cards =
        [
            new CardDetailsDto
            {
                Id = ExistingCardId,
                CardNumber = "**** **** **** 1234",
                FullCardNumber = "4111111111111234",
                SecurityCode = "123",
                CardholderName = "Jane Doe",
                ExpiryDate = expiryDate,
                CardType = CardType.Debit,
                CardBrand = "Visa",
                Status = CardStatus.Active,
                IsContactlessEnabled = true,
                IsOnlineEnabled = false,
                AccountName = "Checking Account",
                AccountIban = "RO49BANK1234567890",
                AccountBalance = 1250.75m,
                AccountCurrency = "USD"
            }
        ];

        _cardServiceMock
            .Setup(service => service.GetCardsAsync(CancellationToken.None))
            .ReturnsAsync(cards);

        // Act
        IActionResult result = await _controller.Index(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        CardListModel viewModel = viewResult.Model.Should().BeOfType<CardListModel>().Subject;

        viewModel.Cards.Should().HaveCount(1);
        CardDetailsDto card = viewModel.Cards[0];
        card.Id.Should().Be(ExistingCardId);
        card.CardNumber.Should().Be("**** **** **** 1234");
        card.FullCardNumber.Should().Be("4111111111111234");
        card.SecurityCode.Should().Be("123");
        card.CardholderName.Should().Be("Jane Doe");
        card.ExpiryDate.Should().Be(expiryDate);
        card.CardType.Should().Be(CardType.Debit);
        card.CardBrand.Should().Be("Visa");
        card.Status.Should().Be(CardStatus.Active);
        card.IsContactlessEnabled.Should().BeTrue();
        card.IsOnlineEnabled.Should().BeFalse();
        card.AccountName.Should().Be("Checking Account");
        card.AccountIban.Should().Be("RO49BANK1234567890");
        card.AccountBalance.Should().Be(1250.75m);
        card.AccountCurrency.Should().Be("USD");
        _cardServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Index_WhenServiceReturnsEmptyList_ShouldReturnViewWithNoCards()
    {
        // Arrange
        _cardServiceMock
            .Setup(service => service.GetCardsAsync(CancellationToken.None))
            .ReturnsAsync(new List<CardDetailsDto>());

        // Act
        IActionResult result = await _controller.Index(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        CardListModel viewModel = viewResult.Model.Should().BeOfType<CardListModel>().Subject;
        viewModel.HasCards.Should().BeFalse();
        _cardServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Index_WhenServiceReturnsError_ShouldSetTempDataErrorAndReturnEmptyView()
    {
        // Arrange
        _cardServiceMock
            .Setup(service => service.GetCardsAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("cards.load_failed", "Unable to load cards."));

        // Act
        IActionResult result = await _controller.Index(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        CardListModel viewModel = viewResult.Model.Should().BeOfType<CardListModel>().Subject;
        viewModel.HasCards.Should().BeFalse();
        _controller.TempData["Error"].Should().Be("Could not load cards. Please try again.");
        _cardServiceMock.VerifyAll();
    }

    // -------------------------------------------------------------------------
    // Issue (POST)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Issue_WhenInvalidModel_ShouldReturnIndexViewWithIssueFormOpenWithoutCallingIssueService()
    {
        // Arrange
        IssueCardModel issueForm = new() { CardBrand = string.Empty };
        _controller.ModelState.AddModelError(nameof(IssueCardModel.CardBrand), "Please select a card brand.");

        _cardServiceMock
            .Setup(service => service.GetCardsAsync(CancellationToken.None))
            .ReturnsAsync(new List<CardDetailsDto>());

        // Act
        IActionResult result = await _controller.Issue(issueForm, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(CardsController.Index));
        CardListModel listModel = viewResult.Model.Should().BeOfType<CardListModel>().Subject;
        listModel.ShowIssueForm.Should().BeTrue();
        listModel.IssueForm.Should().Be(issueForm);
        _cardServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Issue_WhenServiceSucceeds_ShouldSetSuccessTempDataAndRedirectToIndex()
    {
        // Arrange
        IssueCardModel issueForm = new() { CardType = CardType.Debit, CardBrand = "Visa" };

        _cardServiceMock
            .Setup(service => service.IssueCardAsync(
                It.Is<IssueCardRequest>(request =>
                    request.CardType == CardType.Debit && request.CardBrand == "Visa"),
                CancellationToken.None))
            .ReturnsAsync(new CardDetailsDto { Id = ExistingCardId });

        // Act
        IActionResult result = await _controller.Issue(issueForm, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(CardsController.Index));
        _controller.TempData["Success"].Should().Be("Your new Visa Debit card has been issued successfully.");
        _cardServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Issue_WhenServiceReturnsError_ShouldReturnIndexViewWithIssueFormOpenAndModelError()
    {
        // Arrange
        IssueCardModel issueForm = new() { CardType = CardType.Credit, CardBrand = "Mastercard" };

        _cardServiceMock
            .Setup(service => service.IssueCardAsync(It.IsAny<IssueCardRequest>(), CancellationToken.None))
            .ReturnsAsync(Error.Failure("cards.issue_failed", "Issue failed."));

        _cardServiceMock
            .Setup(service => service.GetCardsAsync(CancellationToken.None))
            .ReturnsAsync(new List<CardDetailsDto>());

        // Act
        IActionResult result = await _controller.Issue(issueForm, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(CardsController.Index));
        CardListModel listModel = viewResult.Model.Should().BeOfType<CardListModel>().Subject;
        listModel.ShowIssueForm.Should().BeTrue();
        listModel.IssueForm.Should().Be(issueForm);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Could not issue card. Please try again.");
        _cardServiceMock.VerifyAll();
    }

    // -------------------------------------------------------------------------
    // Freeze
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Freeze_WhenServiceSucceeds_ShouldSetSuccessTempDataAndRedirectToIndex()
    {
        // Arrange
        _cardServiceMock
            .Setup(service => service.FreezeCardAsync(ExistingCardId, CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.Freeze(ExistingCardId, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(CardsController.Index));
        _controller.TempData["Success"].Should().Be("Card has been frozen successfully.");
        _cardServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Freeze_WhenServiceReturnsError_ShouldSetErrorTempDataAndRedirectToIndex()
    {
        // Arrange
        _cardServiceMock
            .Setup(service => service.FreezeCardAsync(ExistingCardId, CancellationToken.None))
            .ReturnsAsync(Error.Failure("cards.freeze_failed", "Freeze failed."));

        // Act
        IActionResult result = await _controller.Freeze(ExistingCardId, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(CardsController.Index));
        _controller.TempData["Error"].Should().Be("Could not freeze the card. Please try again.");
        _cardServiceMock.VerifyAll();
    }

    // -------------------------------------------------------------------------
    // Unfreeze
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Unfreeze_WhenServiceSucceeds_ShouldSetSuccessTempDataAndRedirectToIndex()
    {
        // Arrange
        _cardServiceMock
            .Setup(service => service.UnfreezeCardAsync(ExistingCardId, CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.Unfreeze(ExistingCardId, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(CardsController.Index));
        _controller.TempData["Success"].Should().Be("Card has been unfrozen successfully.");
        _cardServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Unfreeze_WhenServiceReturnsError_ShouldSetErrorTempDataAndRedirectToIndex()
    {
        // Arrange
        _cardServiceMock
            .Setup(service => service.UnfreezeCardAsync(ExistingCardId, CancellationToken.None))
            .ReturnsAsync(Error.Failure("cards.unfreeze_failed", "Unfreeze failed."));

        // Act
        IActionResult result = await _controller.Unfreeze(ExistingCardId, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(CardsController.Index));
        _controller.TempData["Error"].Should().Be("Could not unfreeze the card. Please try again.");
        _cardServiceMock.VerifyAll();
    }

    // -------------------------------------------------------------------------
    // Cancel
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Cancel_WhenServiceSucceeds_ShouldSetSuccessTempDataAndRedirectToIndex()
    {
        // Arrange
        _cardServiceMock
            .Setup(service => service.CancelCardAsync(ExistingCardId, CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.Cancel(ExistingCardId, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(CardsController.Index));
        _controller.TempData["Success"].Should().Be("Card has been cancelled.");
        _cardServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Cancel_WhenServiceReturnsError_ShouldSetErrorTempDataAndRedirectToIndex()
    {
        // Arrange
        _cardServiceMock
            .Setup(service => service.CancelCardAsync(ExistingCardId, CancellationToken.None))
            .ReturnsAsync(Error.Failure("cards.cancel_failed", "Cancel failed."));

        // Act
        IActionResult result = await _controller.Cancel(ExistingCardId, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(CardsController.Index));
        _controller.TempData["Error"].Should().Be("Could not cancel the card. Please try again.");
        _cardServiceMock.VerifyAll();
    }
}

