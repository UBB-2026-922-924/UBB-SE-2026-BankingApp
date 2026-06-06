namespace BankingApp.Desktop.Tests.ViewModels;

using System.Collections.Generic;
using Desktop.ViewModels;
using Contracts.Features.Transfers.Dtos;
using Contracts.Features.Transfers.Services;
using State;
using ErrorOr;
using FluentAssertions;
using Moq;
using Shared;
using Xunit;

/// <summary>
///     Tests for the <see cref="TransferViewModel" />.
/// </summary>
public class TransferViewModelTests
{
    private const int IbanValidationStep = 1;
    private const int TransferDetailsStep = 2;
    private const int ReviewAndConfirmationStep = 3;
    private const int TransferCompletedStep = 4;
    private const int TransferErrorStep = 5;

    private readonly Mock<ITransferService> _transferClientService;
    private readonly TransferViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferViewModelTests" /> class.
    ///     Creates a fresh mock and view model for each test.
    /// </summary>
    public TransferViewModelTests()
    {
        _transferClientService = new Mock<ITransferService>(MockBehavior.Loose);
        _viewModel = new TransferViewModel(_transferClientService.Object, Mock.Of<ITransferDraftState>());
    }

    /// <summary>
    ///     In LoadAccountsAsync, when the API returns accounts, the collection
    ///     should be populated and the first account selected.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadAccountsAsync_WhenApiReturnsAccounts_ShouldPopulateAccountsAndSelectFirst()
    {
        // Arrange
        var accounts = new List<TransferAccountSelectionResponse>
        {
            new TransferAccountSelectionResponse { Id = 1, AccountName = "Main", Currency = "EUR", Balance = 1000m },
            new TransferAccountSelectionResponse { Id = 2, AccountName = "Savings", Currency = "USD", Balance = 500m }
        };

        _transferClientService
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(accounts);

        // Act
        await _viewModel.LoadAccountsAsync();

        // Assert
        _viewModel.Accounts.Should().HaveCount(2);
        _viewModel.SelectedAccount.Should().NotBeNull();
        _viewModel.SelectedAccount!.AccountName.Should().Be("Main");
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     In LoadAccountsAsync, when the API returns an error, the error message
    ///     should be set and the accounts collection should remain empty.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadAccountsAsync_WhenApiFails_ShouldSetErrorMessage()
    {
        // Arrange
        _transferClientService
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.LoadAccountsAsync();

        // Assert
        _viewModel.Accounts.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().Be(UserMessages.Transfer.AccountLoadFailed);
    }

    /// <summary>
    ///     ExecuteNextStep from step 1 should advance to step 2 when IBAN is valid.
    /// </summary>
    [Fact]
    public async Task ExecuteNextStep_WhenOnStep1AndIbanIsValid_ShouldGoToDetailsStep()
    {
        // Arrange
        const string recipientIban = "RO49AAAA1B31007593840000";
        const string bankName = "Test Bank";
        _viewModel.CurrentStep = IbanValidationStep;
        _viewModel.RecipientIban = recipientIban;

        _transferClientService
            .Setup(service => service.ValidateIbanAsync(
                It.Is<TransferIbanValidationRequest>(request => request.Iban == recipientIban),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransferIbanValidationResponse { IsValid = true, BankName = bankName });

        // Act
        await _viewModel.ExecuteNextStep();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferDetailsStep);
        _viewModel.IsIbanValid.Should().BeTrue();
        _viewModel.BankName.Should().Be(bankName);
    }

    /// <summary>
    ///     ExecuteNextStep at the IBAN step when IBAN is invalid should set
    ///     the error step and the invalid IBAN error message.
    /// </summary>
    [Fact]
    public async Task ExecuteNextStep_WhenOnStep1AndIbanIsInvalid_ShouldSetError()
    {
        // Arrange
        const string recipientIban = "INVALID_IBAN";
        _viewModel.CurrentStep = IbanValidationStep;
        _viewModel.RecipientIban = recipientIban;

        _transferClientService
            .Setup(service => service.ValidateIbanAsync(
                It.Is<TransferIbanValidationRequest>(request => request.Iban == recipientIban),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransferIbanValidationResponse { IsValid = false });

        // Act
        await _viewModel.ExecuteNextStep();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferErrorStep);
        _viewModel.IsIbanValid.Should().BeFalse();
        _viewModel.ErrorMessage.Should().Be(UserMessages.Transfer.InvalidIban);
    }

    /// <summary>
    ///     ExecuteNextStep at the details step when amount is zero should set
    ///     the error step and the amount error message.
    /// </summary>
    [Fact]
    public async Task ExecuteNextStep_WhenOnStep2AndAmountIsZero_ShouldSetError()
    {
        // Arrange
        _viewModel.CurrentStep = TransferDetailsStep;
        _viewModel.Amount = 0m;

        // Act
        await _viewModel.ExecuteNextStep();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferErrorStep);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Transfer.AmountMustBePositive);
    }

    [Fact]
    public async Task ExecuteNextStep_WhenOnStep2AndAmountIsPositive_ShouldGoToReview()
    {
        // Arrange
        _viewModel.CurrentStep = TransferDetailsStep;
        _viewModel.SelectedAccount = new TransferAccountSelectionResponse { Id = 1, AccountName = "Main", Currency = "EUR" };
        _viewModel.RecipientName = "Jane Doe";
        _viewModel.Amount = 100m;

        // Act
        await _viewModel.ExecuteNextStep();

        // Assert
        _viewModel.CurrentStep.Should().Be(ReviewAndConfirmationStep);
    }

    /// <summary>
    ///     ExecuteTransferAsync when the API succeeds should set the completed step
    ///     and populate the transaction reference.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteTransferAsync_WhenApiSucceeds_ShouldSetCompletedStep()
    {
        // Arrange
        const string expectedRef = "TXN-20260504-0001";
        _viewModel.SelectedAccount = new TransferAccountSelectionResponse { Id = 1, AccountName = "Main", Currency = "EUR" };
        _viewModel.RecipientName = "Jane Doe";
        _viewModel.Amount = 250m;
        _viewModel.Currency = "EUR";

        _transferClientService
            .Setup(service => service.ExecuteAsync(It.IsAny<CreateTransferRequest>()))
            .ReturnsAsync(new TransferExecutionResponse { TransactionRef = expectedRef });

        // Act
        await _viewModel.ExecuteTransferAsync();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferCompletedStep);
        _viewModel.TransactionRef.Should().Be(expectedRef);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     ExecuteTransferAsync when the API returns an error should set the error step
    ///     and populate the error message.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteTransferAsync_WhenApiFails_ShouldSetErrorStep()
    {
        // Arrange
        const string errorDescription = "Insufficient funds.";
        _viewModel.SelectedAccount = new TransferAccountSelectionResponse { Id = 1, AccountName = "Main", Currency = "EUR" };

        _transferClientService
            .Setup(service => service.ExecuteAsync(It.IsAny<CreateTransferRequest>()))
            .ReturnsAsync(Error.Failure(description: errorDescription));

        // Act
        await _viewModel.ExecuteTransferAsync();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferErrorStep);
        _viewModel.ErrorMessage.Should().Be(errorDescription);
    }

    /// <summary>
    ///     ExecuteSendAgain should reset all form fields and return to step 1.
    /// </summary>
    [Fact]
    public async Task ExecuteSendAgain_WhenCalled_ShouldResetAllFieldsAndReturnToStep1()
    {
        // Arrange - set up accounts and dirty state
        var accounts = new List<TransferAccountSelectionResponse>
        {
            new TransferAccountSelectionResponse { Id = 1, AccountName = "Main", Currency = "EUR" }
        };

        _transferClientService
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(accounts);

        await _viewModel.LoadAccountsAsync();
        _viewModel.RecipientName = "Jane Doe";
        _viewModel.RecipientIban = "RO49AAAA1B31007593840000";
        _viewModel.AmountText = "500";
        _viewModel.Currency = "USD";
        _viewModel.TransactionRef = "TXN-123";
        _viewModel.ErrorMessage = "Some error";
        _viewModel.CurrentStep = TransferCompletedStep;

        // Act
        _viewModel.ExecuteSendAgain();

        // Assert
        _viewModel.CurrentStep.Should().Be(IbanValidationStep);
        _viewModel.RecipientName.Should().BeEmpty();
        _viewModel.RecipientIban.Should().BeEmpty();
        _viewModel.AmountText.Should().BeEmpty();
        _viewModel.Currency.Should().Be("EUR");
        _viewModel.TransactionRef.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.SelectedAccount.Should().NotBeNull();
    }

    /// <summary>
    ///     ExecuteTransferAsync when no account is selected should set the error step.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteTransferAsync_WhenNoAccountSelected_ShouldSetErrorStep()
    {
        // Arrange
        _viewModel.SelectedAccount = null;

        // Act
        await _viewModel.ExecuteTransferAsync();

        // Assert
        _viewModel.CurrentStep.Should().Be(TransferErrorStep);
        _viewModel.ErrorMessage.Should().Be(UserMessages.Transfer.NoAccountSelected);
    }
}
