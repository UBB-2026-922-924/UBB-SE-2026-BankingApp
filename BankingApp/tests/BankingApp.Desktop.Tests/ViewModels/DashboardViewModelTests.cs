namespace BankingApp.Desktop.Tests.ViewModels;

using System.Globalization;
using Contracts.Features.AccountOverview.Dtos;
using Contracts.Features.AccountOverview.Services;
using Desktop.ViewModels;
using Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;
using Shared.Enums;

public class DashboardViewModelTests
{
    private const int CardNumberVisibleSuffixLength = 4;

    private readonly Mock<IAccountOverviewService> _dashboardClientService;
    private readonly DashboardViewModel _viewModel;

    public DashboardViewModelTests()
    {
        _dashboardClientService = new Mock<IAccountOverviewService>(MockBehavior.Strict);
        _viewModel = new DashboardViewModel(_dashboardClientService.Object, NullLogger<DashboardViewModel>.Instance);
    }

    [Fact]
    public async Task LoadDashboard_WhenResponseIsValid_PopulatesViewModel()
    {
        // Arrange
        const string fullName = "Ada Lovelace";
        const string email = "ada@lovelace.com";
        const string cardBrand = "Visa";
        const CardType cardType = CardType.Debit;
        const string cardNumber = "1234567812345678";
        var cardExpiry = new DateTime(2027, 12, 1);
        const string merchantName = "Coffee Shop";
        const string currency = "USD";
        const decimal transactionAmount = 12.5m;
        const int unreadCount = 4;

        var response = new AccountOverviewDto
        {
            CurrentUser = new UserSummaryDto
            {
                FullName = fullName,
                Email = email,
            },
            Cards =
            [
                new CardDto
                {
                    CardBrand = cardBrand,
                    CardType = cardType,
                    CardholderName = fullName,
                    CardNumber = cardNumber,
                    ExpiryDate = cardExpiry,
                    Status = CardStatus.Active,
                    IsContactlessEnabled = true,
                    IsOnlineEnabled = true,
                },
            ],
            RecentTransactions =
            [
                new TransactionDto
                {
                    MerchantName = merchantName,
                    Direction = TransactionDirection.Out,
                    Amount = transactionAmount,
                    Currency = currency,
                },
            ],
            UnreadNotificationCount = unreadCount,
        };
        _dashboardClientService
            .Setup(dashboardClientService => dashboardClientService.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ErrorOr<Success> result = await _viewModel.LoadDashboard(TestContext.Current.CancellationToken);

        // Assert - result and state
        result.IsError.Should().BeFalse();
        _viewModel.State.Should().Be(DashboardState.Success);
        _viewModel.ErrorMessage.Should().BeEmpty();

        // Assert - current user
        _viewModel.CurrentUser.Should().BeEquivalentTo(
            new UserSummaryDto
            {
                FullName = fullName,
                Email = email,
            });

        // Assert - selected card display properties
        _viewModel.CardDots.Should().ContainSingle();
        _viewModel.SelectedCardBrandDisplay.Should().Be(cardBrand);
        _viewModel.SelectedCardHolderDisplay.Should().Be(fullName.ToUpperInvariant());
        _viewModel.SelectedCardNumberMasked.Should()
            .Be($"**** **** **** {cardNumber[^CardNumberVisibleSuffixLength..]}");
        _viewModel.SelectedCardExpiryDisplay.Should().Be(cardExpiry.ToString("MM/yy", CultureInfo.InvariantCulture));

        // Assert - transaction item
        string expectedAmountDisplay = $"-{transactionAmount.ToString("N2", CultureInfo.InvariantCulture)}";
        _viewModel.RecentTransactionItems.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(
                new DashboardTransactionItem
                {
                    MerchantDisplayName = merchantName,
                    AmountDisplay = expectedAmountDisplay,
                    Currency = currency,
                });

        // Assert - notification count
        _viewModel.UnreadNotificationCount.Should().Be(unreadCount);
    }

    [Fact]
    public async Task LoadDashboard_WhenCurrentUserIsMissing_SetsErrorState()
    {
        // Arrange
        _dashboardClientService
            .Setup(service => service.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountOverviewDto());

        // Act
        ErrorOr<Success> result = await _viewModel.LoadDashboard(TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.State.Should().Be(DashboardState.Error);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.IncompleteResponse);
    }

    [Fact]
    public async Task LoadDashboard_WhenUnauthorized_SetsSessionExpiredMessage()
    {
        // Arrange
        _dashboardClientService
            .Setup(service => service.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unauthorized());

        // Act
        ErrorOr<Success> result = await _viewModel.LoadDashboard(TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.State.Should().Be(DashboardState.Error);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.SessionExpired);
    }

    [Fact]
    public async Task LoadDashboard_WhenNotFound_ShouldSetNotFoundMessage()
    {
        // Arrange
        _dashboardClientService
            .Setup(dashboardClientService => dashboardClientService.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound());

        // Act
        ErrorOr<Success> result = await _viewModel.LoadDashboard(TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.State.Should().Be(DashboardState.Error);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.NotFound);
    }

    [Fact]
    public async Task LoadDashboard_WhenApiFailureOccurs_ShouldSetLoadFailedMessage()
    {
        // Arrange
        _dashboardClientService
            .Setup(dashboardClientService => dashboardClientService.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure());

        // Act
        ErrorOr<Success> result = await _viewModel.LoadDashboard(TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.State.Should().Be(DashboardState.Error);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.LoadFailed);
    }

    [Fact]
    public async Task NavigatePrevious_WhenNoCardsAreLoaded_ReturnsError()
    {
        // Arrange
        await LoadViewModelWithCards(0);

        // Act
        ErrorOr<Success> result = _viewModel.NavigatePrevious();

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task NavigatePrevious_WhenAtFirstCard_ReturnsError()
    {
        // Arrange
        await LoadViewModelWithCards(2);

        // Act
        ErrorOr<Success> result = _viewModel.NavigatePrevious();

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.CurrentCardIndex.Should().Be(0);
    }

    [Fact]
    public async Task NavigatePrevious_WhenNotAtFirstCard_ShouldSucceedAndDecrementIndex()
    {
        // Arrange
        await LoadViewModelWithCards(2);
        _viewModel.NavigateNext();

        // Act
        ErrorOr<Success> result = _viewModel.NavigatePrevious();

        // Assert
        result.IsError.Should().BeFalse();
        _viewModel.CurrentCardIndex.Should().Be(0);
    }

    [Fact]
    public async Task NavigateNext_WhenNoCardsAreLoaded_ReturnsError()
    {
        // Arrange
        await LoadViewModelWithCards(0);

        // Act
        ErrorOr<Success> result = _viewModel.NavigateNext();

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateNext_WhenAtLastCard_ReturnsError()
    {
        // Arrange
        await LoadViewModelWithCards(1);

        // Act
        ErrorOr<Success> result = _viewModel.NavigateNext();

        // Assert
        result.IsError.Should().BeTrue();
        _viewModel.CurrentCardIndex.Should().Be(0);
    }

    [Fact]
    public async Task NavigateNext_WhenNotAtLastCard_SucceedsAndIncrementsIndex()
    {
        // Arrange
        await LoadViewModelWithCards(2);

        // Act
        ErrorOr<Success> result = _viewModel.NavigateNext();

        // Assert
        result.IsError.Should().BeFalse();
        _viewModel.CurrentCardIndex.Should().Be(1);
    }

    [Fact]
    public async Task SelectedCardBrandDisplay_WhenBrandIsPresent_ReturnsBrand()
    {
        // Arrange
        const string expectedCardBrand = "Visa";
        await LoadViewModelWithCards(1);

        // Act
        string display = _viewModel.SelectedCardBrandDisplay;

        // Assert
        display.Should().Be(expectedCardBrand);
    }

    [Fact]
    public async Task SelectedCardBrandDisplay_WhenBrandIsAbsent_ShouldFallBackToCardType()
    {
        // Arrange
        const CardType cardType = CardType.Credit;
        await LoadViewModelWithCards(1, string.Empty, cardType);

        // Act
        string display = _viewModel.SelectedCardBrandDisplay;

        // Assert
        display.Should().Be(cardType.ToString());
    }

    [Fact]
    public async Task SelectedCardHolderDisplay_WhenNameIsPresent_ShouldReturnUpperCasedName()
    {
        // Arrange
        const string cardholderName = "Ada Lovelace";
        await LoadViewModelWithCards(1, cardholderName: cardholderName);

        // Act
        string display = _viewModel.SelectedCardHolderDisplay;

        // Assert
        display.Should().Be(cardholderName.ToUpperInvariant());
    }

    [Fact]
    public async Task SelectedCardHolderDisplay_WhenNameIsAbsent_ReturnsPlaceholder()
    {
        // Arrange
        const string expectedSelectedCardHolderDisplay = "CARD HOLDER";
        await LoadViewModelWithCards(1, cardholderName: string.Empty);

        // Act
        string display = _viewModel.SelectedCardHolderDisplay;

        // Assert
        display.Should().Be(expectedSelectedCardHolderDisplay);
    }

    [Fact]
    public async Task SelectedCardNumberMasked_WhenCardNumberIsValid_ShowsOnlyLastFourDigits()
    {
        // Arrange
        const string cardNumber = "1234567890123456";
        string expectedMaskedCardNumber = $"**** **** **** {cardNumber[^CardNumberVisibleSuffixLength..]}";
        await LoadViewModelWithCards(1, cardNumber: cardNumber);

        // Act
        string masked = _viewModel.SelectedCardNumberMasked;

        // Assert
        masked.Should().Be(expectedMaskedCardNumber);
    }

    [Fact]
    public async Task SelectedCardNumberMasked_WhenCardNumberIsTooShort_ShouldReturnFullyMasked()
    {
        // Arrange
        const string expectedMaskedCardNumber = "**** **** **** ****";
        await LoadViewModelWithCards(1, cardNumber: "123");

        // Act
        string masked = _viewModel.SelectedCardNumberMasked;

        // Assert
        masked.Should().Be(expectedMaskedCardNumber);
    }

    [Fact]
    public void GetSelectedCardDetails_WhenNoCardIsSelected_ReturnsEmptyString()
    {
        string details = _viewModel.GetSelectedCardDetails();

        details.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSelectedCardDetails_WhenCardIsSelected_ShouldReturnFormattedDetails()
    {
        // Arrange
        const CardType cardType = CardType.Debit;
        const string cardBrand = "Visa";
        const string cardNumber = "1234567890123456";
        const string cardholderName = "Ada Lovelace";
        string expectedMaskedCardNumber = $"**** **** **** {cardNumber[^CardNumberVisibleSuffixLength..]}";
        await LoadViewModelWithCards(
            cardCount: 1,
            cardBrand: cardBrand,
            cardType: cardType,
            cardholderName: cardholderName,
            cardNumber: cardNumber);

        // Act
        string details = _viewModel.GetSelectedCardDetails();

        // Assert
        details.Should().Contain(cardType.ToString())
            .And.Contain(cardBrand)
            .And.Contain(expectedMaskedCardNumber)
            .And.Contain(cardholderName);
    }

    [Fact]
    public async Task CardDots_WhenNavigatedToSecondCard_ShouldActivateSecondDot()
    {
        // Arrange
        await LoadViewModelWithCards(3);
        _viewModel.NavigateNext();

        // Act
        IReadOnlyList<CardPageIndicatorViewModel> dots = _viewModel.CardDots;

        // Assert
        dots.Select(dot => dot.IsActive).Should().Equal(false, true, false);
    }

    private async Task LoadViewModelWithCards(
        int cardCount,
        string cardBrand = "Visa",
        CardType cardType = CardType.Debit,
        string cardholderName = "Test User",
        string cardNumber = "1234567812345678")
    {
        var cards = Enumerable.Range(0, cardCount)
            .Select(_ => new CardDto
            {
                CardBrand = cardBrand,
                CardType = cardType,
                CardholderName = cardholderName,
                CardNumber = cardNumber,
            })
            .ToList();

        var response = new AccountOverviewDto
        {
            CurrentUser = new UserSummaryDto { FullName = "Test User" },
            Cards = cards,
        };

        _dashboardClientService
            .Setup(dashboardClientService => dashboardClientService.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        await _viewModel.LoadDashboard(TestContext.Current.CancellationToken);
    }
}
