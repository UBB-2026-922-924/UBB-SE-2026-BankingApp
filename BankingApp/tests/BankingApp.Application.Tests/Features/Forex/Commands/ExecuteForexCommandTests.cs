namespace BankingApp.Application.Tests.Features.Forex.Commands;

using BankingApp.Application;
using BankingApp.Application.Features.Forex.Services;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.Forex.Dtos;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;
using Currency = NodaMoney.Currency;
using Money = NodaMoney.Money;

public sealed class ExecuteForexCommandTests
{
    private const int TestUserId = 1;
    private const int OtherUserId = 2;
    private const int SourceAccountId = 100;
    private const int TargetAccountId = 200;
    private const decimal SourceAmount = 100m;
    private const decimal ExchangeRate = 1.25m;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IAccountRepository> _accountRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IForexRepository> _forexRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<ILockedRateCache> _lockedRateCacheMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);
    private readonly Mock<IExchangeRateService> _exchangeRateServiceMock = new();

    [Fact]
    public async Task Handle_WhenSourceAccountNotFound_ShouldReturnAccountNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        SetupValidLockedRate();

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync((Account?)null);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.NotFound);

        VerifyValidLockedRateAccess();
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenTargetAccountNotFound_ShouldReturnAccountNotFoundError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Account sourceAccount = CreateAccount(TestUserId, Currency.FromCode("EUR"), 200m);
        SetupValidLockedRate();

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync(sourceAccount);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TargetAccountId, cancellationToken))
            .ReturnsAsync((Account?)null);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.NotFound);

        VerifyValidLockedRateAccess();
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(TargetAccountId, cancellationToken), Times.Once);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenLockedRateNotFound_ShouldReturnRateExpiredError()
    {
        // Arrange
        _lockedRateCacheMock
            .Setup(cache => cache.TryGet(TestUserId))
            .Returns((LockedRate?)null);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.RateExpired);

        _lockedRateCacheMock.Verify(cache => cache.TryGet(TestUserId), Times.Once);
        _clockMock.VerifyNoOtherCalls();
        _accountRepositoryMock.VerifyNoOtherCalls();
        _forexRepositoryMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenLockedRateCurrencyMismatches_ShouldReturnLockedRateMismatchError()
    {
        // Arrange
        var sourceCurrency = Currency.FromCode("EUR");
        var targetCurrency = Currency.FromCode("GBP");

        _lockedRateCacheMock
            .Setup(cache => cache.TryGet(TestUserId))
            .Returns(new LockedRate(sourceCurrency, targetCurrency, ExchangeRate, _testNow));

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.LockedRateMismatch);

        _lockedRateCacheMock.Verify(cache => cache.TryGet(TestUserId), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _accountRepositoryMock.VerifyNoOtherCalls();
        _forexRepositoryMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSourceAccountCurrencyMismatches_ShouldReturnAccountCurrencyMismatchError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Account sourceAccount = CreateAccount(TestUserId, Currency.FromCode("GBP"), 200m);
        Account targetAccount = CreateAccount(TestUserId, Currency.FromCode("USD"), 0m);
        SetupValidLockedRate();
        SetupAccountLookups(sourceAccount, targetAccount, cancellationToken);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.AccountCurrencyMismatch);

        VerifyValidLockedRateAccess();
        VerifyAccountLookups(cancellationToken);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenTargetAccountCurrencyMismatches_ShouldReturnAccountCurrencyMismatchError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Account sourceAccount = CreateAccount(TestUserId, Currency.FromCode("EUR"), 200m);
        Account targetAccount = CreateAccount(TestUserId, Currency.FromCode("GBP"), 0m);
        SetupValidLockedRate();
        SetupAccountLookups(sourceAccount, targetAccount, cancellationToken);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.AccountCurrencyMismatch);

        VerifyValidLockedRateAccess();
        VerifyAccountLookups(cancellationToken);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenInsufficientFundsInSourceAccount_ShouldReturnInsufficientFundsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Account sourceAccount = CreateAccount(TestUserId, Currency.FromCode("EUR"), 50m);
        Account targetAccount = CreateAccount(TestUserId, Currency.FromCode("USD"), 0m);
        SetupValidLockedRate();
        SetupAccountLookups(sourceAccount, targetAccount, cancellationToken);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.InsufficientFunds);

        VerifyValidLockedRateAccess(expectedClockCalls: 2);
        VerifyAccountLookups(cancellationToken);
        VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldDebitSourceAndCreditTargetAccount()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Account sourceAccount = CreateAccount(TestUserId, Currency.FromCode("EUR"), 200m);
        Account targetAccount = CreateAccount(TestUserId, Currency.FromCode("USD"), 0m);
        ForexTransaction? persistedForex = null;
        SetupValidCommand(sourceAccount, targetAccount, cancellationToken, forex => persistedForex = forex);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        sourceAccount.Balance.Amount.Should().Be(99.50m);
        targetAccount.Balance.Amount.Should().Be(125m);
        sourceAccount.Transactions.Should().ContainSingle();
        targetAccount.Transactions.Should().ContainSingle();
        persistedForex.Should().NotBeNull();

        VerifyValidCommand(sourceAccount, targetAccount, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldMarkForexTransactionAsExecuted()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Account sourceAccount = CreateAccount(TestUserId, Currency.FromCode("EUR"), 200m);
        Account targetAccount = CreateAccount(TestUserId, Currency.FromCode("USD"), 0m);
        ForexTransaction? persistedForex = null;
        SetupValidCommand(sourceAccount, targetAccount, cancellationToken, forex => persistedForex = forex);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        persistedForex.Should().NotBeNull();
        persistedForex!.Status.Should().Be(ExchangeTransactionStatus.Completed);
        persistedForex.SourceLedgerTransactionId.Should().NotBeNull();
        persistedForex.TargetLedgerTransactionId.Should().NotBeNull();
        result.Value.Status.Should().Be(ExchangeTransactionStatus.Completed);

        VerifyValidCommand(sourceAccount, targetAccount, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldRemoveLockedRateFromCache()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Account sourceAccount = CreateAccount(TestUserId, Currency.FromCode("EUR"), 200m);
        Account targetAccount = CreateAccount(TestUserId, Currency.FromCode("USD"), 0m);
        SetupValidCommand(sourceAccount, targetAccount, cancellationToken);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();

        VerifyValidCommand(sourceAccount, targetAccount, cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Account sourceAccount = CreateAccount(TestUserId, Currency.FromCode("EUR"), 200m);
        Account targetAccount = CreateAccount(TestUserId, Currency.FromCode("USD"), 0m);
        SetupValidCommand(sourceAccount, targetAccount, cancellationToken);

        ForexService service = CreateService();

        // Act
        ErrorOr<ForexTransactionResponse> result = await service.ExecuteAsync(TestUserId, SourceAccountId, TargetAccountId, "EUR", "USD", SourceAmount, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.SourceCurrency.Should().Be("EUR");
        result.Value.TargetCurrency.Should().Be("USD");
        result.Value.TargetAmount.Should().Be(125m);
        result.Value.ExchangeRate.Should().Be(ExchangeRate);
        result.Value.Commission.Should().Be(0.50m);

        VerifyValidCommand(sourceAccount, targetAccount, cancellationToken);
    }

    private ForexService CreateService()
    {
        return new ForexService(
            _accountRepositoryMock.Object,
            _forexRepositoryMock.Object,
            _lockedRateCacheMock.Object,
            _exchangeRateServiceMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object);
    }

    private void SetupValidLockedRate()
    {
        _lockedRateCacheMock
            .Setup(cache => cache.TryGet(TestUserId))
            .Returns(new LockedRate(Currency.FromCode("EUR"), Currency.FromCode("USD"), ExchangeRate, _testNow));

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);
    }

    private void SetupAccountLookups(Account sourceAccount, Account targetAccount, CancellationToken cancellationToken)
    {
        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken))
            .ReturnsAsync(sourceAccount);

        _accountRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TargetAccountId, cancellationToken))
            .ReturnsAsync(targetAccount);
    }

    private void SetupValidCommand(
        Account sourceAccount,
        Account targetAccount,
        CancellationToken cancellationToken,
        Action<ForexTransaction>? onPersist = null)
    {
        SetupValidLockedRate();
        SetupAccountLookups(sourceAccount, targetAccount, cancellationToken);

        _accountRepositoryMock
            .Setup(repository => repository.UpdateAsync(sourceAccount, cancellationToken))
            .Returns(Task.CompletedTask);

        _accountRepositoryMock
            .Setup(repository => repository.UpdateAsync(targetAccount, cancellationToken))
            .Returns(Task.CompletedTask);

        if (onPersist is null)
        {
            _forexRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<ForexTransaction>(), cancellationToken))
                .Returns(Task.CompletedTask);
        }
        else
        {
            _forexRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<ForexTransaction>(), cancellationToken))
                .Callback<ForexTransaction, CancellationToken>((forex, _) => onPersist(forex))
                .Returns(Task.CompletedTask);
        }

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _lockedRateCacheMock.Setup(cache => cache.Remove(TestUserId));
    }

    private void VerifyValidLockedRateAccess(int expectedClockCalls = 1)
    {
        _lockedRateCacheMock.Verify(cache => cache.TryGet(TestUserId), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Exactly(expectedClockCalls));
    }

    private void VerifyAccountLookups(CancellationToken cancellationToken)
    {
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(SourceAccountId, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.GetByIdAsync(TargetAccountId, cancellationToken), Times.Once);
    }

    private void VerifyValidCommand(Account sourceAccount, Account targetAccount, CancellationToken cancellationToken)
    {
        VerifyValidLockedRateAccess(expectedClockCalls: 2);
        VerifyAccountLookups(cancellationToken);
        _accountRepositoryMock.Verify(repository => repository.UpdateAsync(sourceAccount, cancellationToken), Times.Once);
        _accountRepositoryMock.Verify(repository => repository.UpdateAsync(targetAccount, cancellationToken), Times.Once);
        _forexRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<ForexTransaction>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _lockedRateCacheMock.Verify(cache => cache.Remove(TestUserId), Times.Once);
        VerifyNoOtherCalls();
    }

    private void VerifyNoOtherCalls()
    {
        _accountRepositoryMock.VerifyNoOtherCalls();
        _forexRepositoryMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private static Account CreateAccount(int userId, Currency currency, decimal balance)
    {
        var account = Account.Open(
            userId,
            Iban.Create("RO12BANK1234567890123456").Value,
            currency,
            AccountType.Checking,
            "Main",
            _testNow);

        account.ChangeBalance(new Money(balance, currency), _testNow);
        return account;
    }
}
