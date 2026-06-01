namespace BankingApp.Desktop.Tests.ViewModels;

using Contracts.Features.Forex.Dtos;
using Contracts.Features.Forex.Services;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.BillPayments.Dtos;
using Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;

public class ForexViewModelTests
{
    private readonly Mock<IAuthenticationSession> _authenticationSession;
    private readonly Mock<IBillPaymentService> _billPaymentService;
    private readonly Mock<IForexService> _forexClientService;
    private readonly ForexViewModel _viewModel;

    public ForexViewModelTests()
    {
        _authenticationSession = new Mock<IAuthenticationSession>(MockBehavior.Loose);
        _billPaymentService = new Mock<IBillPaymentService>(MockBehavior.Loose);
        _forexClientService = new Mock<IForexService>(MockBehavior.Loose);
        _viewModel = new ForexViewModel(
            _authenticationSession.Object,
            _billPaymentService.Object,
            _forexClientService.Object,
            NullLogger<ForexViewModel>.Instance);
    }

    [Fact]
    public async Task LoadPreviewAsync_WhenSourceCurrencyIsEmpty_SetsError()
    {
        // Arrange
        _viewModel.SelectedSourceAccount = new AccountDto { Id = 1 };
        _viewModel.SelectedTargetAccount = new AccountDto { Id = 2 };
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.CurrencyRequired);
    }

    [Fact]
    public async Task LoadPreviewAsync_WhenTargetCurrencyIsEmpty_SetsError()
    {
        // Arrange
        _viewModel.SelectedSourceAccount = new AccountDto { Id = 1 };
        _viewModel.SelectedTargetAccount = new AccountDto { Id = 2 };
        _viewModel.SourceCurrency = "EUR";
        _viewModel.AmountText = "100";

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.CurrencyRequired);
    }

    [Fact]
    public async Task LoadPreviewAsync_WhenAmountIsZero_SetsError()
    {
        // Arrange
        _viewModel.SelectedSourceAccount = new AccountDto { Id = 1 };
        _viewModel.SelectedTargetAccount = new AccountDto { Id = 2 };
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "0";

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.AmountRequired);
    }

    [Fact]
    public async Task LoadPreviewAsync_WhenApiSucceeds_PopulatesRateData()
    {
        // Arrange
        const decimal expectedRate = 1.12m;
        const decimal expectedCommission = 0.50m;
        const decimal expectedTargetAmount = 111.50m;

        _viewModel.SelectedSourceAccount = new AccountDto { Id = 1 };
        _viewModel.SelectedTargetAccount = new AccountDto { Id = 2 };
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        var response = new ForexRatePreviewResponse
        {
            ExchangeRate = expectedRate,
            Commission = expectedCommission,
            TargetAmount = expectedTargetAmount,
        };

        _forexClientService
            .Setup(service => service.GetPreviewAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.LiveRate.Should().Be(expectedRate);
        _viewModel.Commission.Should().Be(expectedCommission);
        _viewModel.TargetAmount.Should().Be(expectedTargetAmount);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadPreviewAsync_WhenApiFails_SetsErrorMessage()
    {
        // Arrange
        _viewModel.SelectedSourceAccount = new AccountDto { Id = 1 };
        _viewModel.SelectedTargetAccount = new AccountDto { Id = 2 };
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        _forexClientService
            .Setup(forexClientService => forexClientService.GetPreviewAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.PreviewFailed);
    }

    [Fact]
    public async Task ExecuteExchangeAsync_WhenAmountIsZero_SetsError()
    {
        // Arrange
        _viewModel.SelectedSourceAccount = new AccountDto { Id = 1 };
        _viewModel.SelectedTargetAccount = new AccountDto { Id = 2 };
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "0";

        // Act
        await _viewModel.ExecuteExchangeAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.AmountRequired);
    }

    [Fact]
    public async Task ExecuteExchangeAsync_WhenApiSucceeds_SetsTransactionReference()
    {
        // Arrange
        const int transactionId = 42;
        _viewModel.SelectedSourceAccount = new AccountDto { Id = 1 };
        _viewModel.SelectedTargetAccount = new AccountDto { Id = 2 };
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";
        _authenticationSession.Setup(service => service.CurrentUserId).Returns(1);

        var response = new ForexTransactionResponse { Id = transactionId };
        _forexClientService
            .Setup(forexClientService => forexClientService.ExecuteAsync(It.IsAny<ForexTransactionRequest>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.ExecuteExchangeAsync();

        // Assert
        _viewModel.TransactionReference.Should().Be($"TX-{transactionId}");
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteExchangeAsync_WhenApiFails_SetsErrorMessage()
    {
        // Arrange
        _viewModel.SelectedSourceAccount = new AccountDto { Id = 1 };
        _viewModel.SelectedTargetAccount = new AccountDto { Id = 2 };
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";
        _authenticationSession.Setup(forexClientService => forexClientService.CurrentUserId).Returns(1);

        _forexClientService
            .Setup(forexClientService => forexClientService.ExecuteAsync(It.IsAny<ForexTransactionRequest>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.ExecuteExchangeAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.ExecuteFailed);
    }

    [Fact]
    public void Reset_ClearsAllState()
    {
        // Arrange
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        // Act
        _viewModel.Reset();

        // Assert
        _viewModel.SourceCurrency.Should().BeEmpty();
        _viewModel.TargetCurrency.Should().BeEmpty();
        _viewModel.AmountText.Should().BeEmpty();
        _viewModel.LiveRate.Should().Be(0);
        _viewModel.Commission.Should().Be(0);
        _viewModel.TargetAmount.Should().Be(0);
        _viewModel.TransactionReference.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.CurrentStep.Should().Be(1);
    }
}
