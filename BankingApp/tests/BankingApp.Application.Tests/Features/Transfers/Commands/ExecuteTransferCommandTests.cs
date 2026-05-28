namespace BankingApp.Application.Tests.Features.Transfers.Commands;

using BankingApp.Application.Features.Transfers.Services;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.Transfers.Dtos;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;
using Currency = NodaMoney.Currency;
using Money = NodaMoney.Money;

public sealed class ExecuteTransferCommandTests
{
    private const int TestUserId = 1;
    private const int OtherUserId = 2;
    private const int SourceAccountId = 100;
    private const string RecipientName = "Jane Doe";
    private const string RecipientIban = "RO12BANK1234567890123456";
    private const string CurrencyCode = "RON";

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IAccountRepository> _accountRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<ITransferRepository> _transferRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);
    private readonly Mock<BankingApp.Application.IExchangeRateService> _exchangeRateServiceMock = new();

    [Fact]
    public async Task Handle_WhenIbanIsInvalid_ShouldReturnInvalidIbanError()
    {
        // Arrange
        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, "invalid-iban", 100m, CurrencyCode, "Invoice", TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.InvalidIban);

        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenCurrencyCodeIsInvalid_ShouldReturnInvalidCurrencyError()
    {
        // Arrange
        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, RecipientIban, 100m, "INVALID", "Invoice", TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.InvalidCurrency);

        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ShouldReturnAccountNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync((Account?)null);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, RecipientIban, 100m, CurrencyCode, "Invoice", cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.AccountNotFound);

        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenAccountBelongsToDifferentUser_ShouldReturnAccountNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Account account = CreateAccount(OtherUserId, balance: 500m);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync(account);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, RecipientIban, 100m, CurrencyCode, "Invoice", cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.AccountNotFound);

        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenAccountIsNotActive_ShouldReturnAccountNotActiveError()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Account account = CreateAccount(TestUserId, balance: 500m);
        SetAccountStatus(account, AccountStatus.Closed);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync(account);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, RecipientIban, 100m, CurrencyCode, "Invoice", cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.AccountNotActive);

        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenAccountCurrencyMismatches_ShouldReturnCurrencyMismatchError()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Account account = CreateAccount(TestUserId, balance: 500m, currencyCode: "EUR");

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync(account);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, RecipientIban, 100m, CurrencyCode, "Invoice", cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.CurrencyMismatch);

        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenInsufficientFunds_ShouldReturnInsufficientFundsError()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Account account = CreateAccount(TestUserId, balance: 50m);
        SetupAccountLookup(account, cancellationToken);
        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);
        _accountRepositoryMock
            .Setup(repository => repository.GetByIbanAsync(RecipientIban, cancellationToken))
            .ReturnsAsync((Account?)null);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, RecipientIban, 100m, CurrencyCode, "Invoice", cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.InsufficientFunds);

        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIbanAsync(RecipientIban, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldDebitAccountAndCreateTransfer()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Account account = CreateAccount(TestUserId, balance: 500m);
        Transfer? persistedTransfer = null;
        SetupValidFlow(account, cancellationToken, transfer => persistedTransfer = transfer);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, RecipientIban, 100m, CurrencyCode, "Invoice", cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        account.Balance.Amount.Should().Be(399m);
        persistedTransfer.Should().NotBeNull();
        persistedTransfer!.Status.Should().Be(TransferStatus.Completed);
        persistedTransfer.Amount.Amount.Should().Be(100m);
        persistedTransfer.Fee.Amount.Should().Be(1m);
        result.Value.Status.Should().Be(TransferStatus.Completed);

        VerifyValidFlow(account, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenRecipientIbanBelongsToLocalAccount_ShouldCreditRecipientAccount()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Account sourceAccount = CreateAccount(TestUserId, balance: 500m, iban: "RO49AAAA1B31007593840000");
        Account recipientAccount = CreateAccount(OtherUserId, balance: 25m, iban: RecipientIban);
        SetAccountId(sourceAccount, SourceAccountId);
        SetAccountId(recipientAccount, SourceAccountId + 1);

        SetupAccountLookup(sourceAccount, cancellationToken);
        _accountRepositoryMock
            .Setup(repository => repository.GetByIbanAsync(RecipientIban, cancellationToken))
            .ReturnsAsync(recipientAccount);
        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _beneficiaryRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync([]);

        _accountRepositoryMock
            .Setup(repository => repository.UpdateAsync(sourceAccount, cancellationToken))
            .Returns(Task.CompletedTask);
        _accountRepositoryMock
            .Setup(repository => repository.UpdateAsync(recipientAccount, cancellationToken))
            .Returns(Task.CompletedTask);

        _transferRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Transfer>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, RecipientIban, 100m, CurrencyCode, "Invoice", cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        sourceAccount.Balance.Amount.Should().Be(399m);
        recipientAccount.Balance.Amount.Should().Be(125m);

        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIbanAsync(RecipientIban, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _beneficiaryRepositoryMock.Verify(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.UpdateAsync(sourceAccount, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.UpdateAsync(recipientAccount, cancellationToken), Times.Once);
        _transferRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Transfer>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Account account = CreateAccount(TestUserId, balance: 500m);
        SetupValidFlow(account, cancellationToken);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, RecipientIban, 100m, CurrencyCode, "Invoice", cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();

        VerifyValidFlow(account, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenBeneficiaryWithRecipientIbanExists_ShouldUpdateBeneficiaryStats()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Account account = CreateAccount(TestUserId, balance: 500m);
        var beneficiary = Beneficiary.Create(
            TestUserId,
            RecipientName,
            Iban.Create(RecipientIban).Value,
            null,
            _testNow);

        SetupAccountLookup(account, cancellationToken);
        _accountRepositoryMock
            .Setup(repository => repository.GetByIbanAsync(RecipientIban, cancellationToken))
            .ReturnsAsync((Account?)null);
        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _beneficiaryRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync([beneficiary]);

        _beneficiaryRepositoryMock
            .Setup(repository => repository.UpdateAsync(beneficiary, cancellationToken))
            .Returns(Task.CompletedTask);

        _accountRepositoryMock
            .Setup(repository => repository.UpdateAsync(account, cancellationToken))
            .Returns(Task.CompletedTask);

        _transferRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Transfer>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        TransferService service = CreateService();

        // Act
        ErrorOr<TransferResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, RecipientName, RecipientIban, 100m, CurrencyCode, "Invoice", cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        beneficiary.TransferCount.Should().Be(1);
        beneficiary.TotalAmountSent.Should().Be(100m);
        beneficiary.LastTransferDate.Should().Be(_testNow);

        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIbanAsync(RecipientIban, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _beneficiaryRepositoryMock.Verify(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _beneficiaryRepositoryMock.Verify(repository => repository.UpdateAsync(beneficiary, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.UpdateAsync(account, cancellationToken), Times.Once);
        _transferRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Transfer>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    private TransferService CreateService()
    {
        return new TransferService(
            _accountRepositoryMock.Object,
            _transferRepositoryMock.Object,
            _beneficiaryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            _exchangeRateServiceMock.Object,
            NullLogger<TransferService>.Instance);
    }

    private void SetupAccountLookup(Account account, CancellationToken cancellationToken)
    {
        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync(account);
    }

    private void SetupValidFlow(
        Account account,
        CancellationToken cancellationToken,
        Action<Transfer>? onPersist = null)
    {
        SetupAccountLookup(account, cancellationToken);
        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIbanAsync(RecipientIban, cancellationToken))
            .ReturnsAsync((Account?)null);

        _beneficiaryRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken))
            .ReturnsAsync([]);

        _accountRepositoryMock
            .Setup(repository => repository.UpdateAsync(account, cancellationToken))
            .Returns(Task.CompletedTask);

        if (onPersist is null)
        {
            _transferRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<Transfer>(), cancellationToken))
                .Returns(Task.CompletedTask);
        }
        else
        {
            _transferRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<Transfer>(), cancellationToken))
                .Callback<Transfer, CancellationToken>((transfer, _) => onPersist(transfer))
                .Returns(Task.CompletedTask);
        }

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);
    }

    private void VerifyValidFlow(Account account, CancellationToken cancellationToken)
    {
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIbanAsync(RecipientIban, cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _beneficiaryRepositoryMock.Verify(repository => repository.ListByUserIdAsync(TestUserId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.UpdateAsync(account, cancellationToken), Times.Once);
        _transferRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Transfer>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    private void VerifyNoOtherCalls()
    {
        _accountRepositoryMock.VerifyNoOtherCalls();
        _transferRepositoryMock.VerifyNoOtherCalls();
        _beneficiaryRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private static Account CreateAccount(
        int userId,
        decimal balance,
        string currencyCode = CurrencyCode,
        string iban = "RO12BANK1234567890123456")
    {
        var currency = Currency.FromCode(currencyCode);
        var account = Account.Open(
            userId,
            Iban.Create(iban).Value,
            currency,
            AccountType.Checking,
            "Main",
            _testNow);

        account.ChangeBalance(new Money(balance, currency), _testNow);
        return account;
    }

    private static void SetAccountStatus(Account account, AccountStatus status)
    {
        typeof(Account)
            .GetProperty(nameof(Account.Status))!
            .SetValue(account, status);
    }

    private static void SetAccountId(Account account, int id)
    {
        typeof(Account)
            .GetProperty(nameof(Account.Id))!
            .SetValue(account, id);
    }
}
