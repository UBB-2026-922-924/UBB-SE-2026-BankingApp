namespace BankingApp.Application.Tests.Features.Savings.Services;

using Application.Features.Savings.Services;
using Domain.Aggregates.SavingsAggregate;
using Domain.Aggregates.SavingsAggregate.Entities;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Shared.Persistence;

public sealed class SavingsServiceTests
{
    private const int TestUserId = 1;
    private const int AccountId = 10;
    private const int DestinationAccountId = 20;

    private static readonly DateTime _createdAt = new(2026, 6, 5, 8, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISavingsRepository> _savingsRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Loose);

    [Fact]
    public async Task CreateAccountAsync_WhenActiveAccountsLimitReached_ShouldReturnValidationError()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        SavingsAccount[] activeAccounts = Enumerable.Range(0, 5)
            .Select(_ => CreateAccount(AccountId, SavingsType.HighYield))
            .ToArray();

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, false, cancellationToken))
            .ReturnsAsync(activeAccounts);

        SavingsService service = CreateService();

        ErrorOr<SavingsAccount> result = await service.CreateAccountAsync(
            TestUserId,
            SavingsType.HighYield,
            "Rainy day",
            null,
            0m,
            null,
            null,
            null,
            null,
            cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Savings.MaxAccountsReached");
    }

    [Fact]
    public async Task CreateAccountAsync_WhenGoalSavingsHasMissingTargetDate_ShouldReturnValidationError()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, false, cancellationToken))
            .ReturnsAsync(Array.Empty<SavingsAccount>());

        SavingsService service = CreateService();

        ErrorOr<SavingsAccount> result = await service.CreateAccountAsync(
            TestUserId,
            SavingsType.GoalSavings,
            "Vacation",
            null,
            0m,
            1000m,
            null,
            null,
            null,
            cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Savings.InvalidTargetDate");
    }

    [Fact]
    public async Task CreateAccountAsync_WhenGoalSavingsHasNoTargetAmount_ShouldReturnValidationError()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, false, cancellationToken))
            .ReturnsAsync(Array.Empty<SavingsAccount>());

        SavingsService service = CreateService();

        ErrorOr<SavingsAccount> result = await service.CreateAccountAsync(
            TestUserId,
            SavingsType.GoalSavings,
            "Vacation",
            null,
            0m,
            null,
            DateTime.Today.AddMonths(6),
            null,
            null,
            cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Savings.InvalidTargetAmount");
    }

    [Fact]
    public async Task CreateAccountAsync_WhenFixedDepositIsRequested_ShouldPersistAccountWithFixedDepositApy()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        SavingsAccount createdAccount = CreateAccount(AccountId, SavingsType.FixedDeposit);

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, false, cancellationToken))
            .ReturnsAsync(Array.Empty<SavingsAccount>());

        _savingsRepositoryMock
            .Setup(repository => repository.CreateSavingsAccountAsync(It.Is<SavingsAccount>(account => account.AnnualPercentageYield == 0.04m), cancellationToken))
            .ReturnsAsync(createdAccount);

        SavingsService service = CreateService();

        ErrorOr<SavingsAccount> result = await service.CreateAccountAsync(
            TestUserId,
            SavingsType.FixedDeposit,
            "Term deposit",
            null,
            0m,
            null,
            null,
            DateTime.UtcNow.AddYears(1),
            null,
            cancellationToken);

        result.IsError.Should().BeFalse();
        _savingsRepositoryMock.Verify(
            repository => repository.CreateSavingsAccountAsync(
                It.Is<SavingsAccount>(account => account.AnnualPercentageYield == 0.04m),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task DepositAsync_WhenAmountIsNegative_ShouldReturnInvalidDepositAmount()
    {
        SavingsService service = CreateService();

        ErrorOr<(decimal, int, DateTime)> result = await service.DepositAsync(TestUserId, AccountId, -100m, "External", TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SavingsErrors.InvalidDepositAmount);
    }

    [Fact]
    public async Task DepositAsync_WhenAccountDoesNotBelongToUser_ShouldReturnAccountNotFound()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, true, cancellationToken))
            .ReturnsAsync([CreateAccount(AccountId + 1, SavingsType.HighYield)]);

        SavingsService service = CreateService();

        ErrorOr<(decimal, int, DateTime)> result = await service.DepositAsync(TestUserId, AccountId, 100m, "External", cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SavingsErrors.AccountNotFound);
    }

    [Fact]
    public async Task DepositAsync_WhenAccountExists_ShouldCallRepositoryDeposit()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        DateTime timestamp = _createdAt.AddHours(1);

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, true, cancellationToken))
            .ReturnsAsync([CreateAccount(AccountId, SavingsType.HighYield)]);

        _savingsRepositoryMock
            .Setup(repository => repository.DepositAsync(AccountId, 100m, "External", cancellationToken))
            .ReturnsAsync((1100m, 55, timestamp));

        SavingsService service = CreateService();

        ErrorOr<(decimal NewBalance, int TransactionId, DateTime Timestamp)> result =
            await service.DepositAsync(TestUserId, AccountId, 100m, "External", cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.NewBalance.Should().Be(1100m);
        result.Value.TransactionId.Should().Be(55);
        result.Value.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public async Task WithdrawAsync_WhenAmountExceedsBalance_ShouldReturnInsufficientBalance()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        SavingsAccount account = CreateAccount(AccountId, SavingsType.HighYield);
        account.Deposit(100m);

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, true, cancellationToken))
            .ReturnsAsync([account]);

        SavingsService service = CreateService();

        ErrorOr<(decimal, decimal, decimal)> result =
            await service.WithdrawAsync(TestUserId, AccountId, 500m, "Checking", cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SavingsErrors.InsufficientBalance);
    }

    [Fact]
    public async Task WithdrawAsync_WhenFixedDepositIsClosedEarly_ShouldApplyPenalty()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        SavingsAccount account = CreateAccount(AccountId, SavingsType.FixedDeposit, DateTime.UtcNow.AddYears(1));
        account.Deposit(200m);

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, true, cancellationToken))
            .ReturnsAsync([account]);

        _savingsRepositoryMock
            .Setup(repository => repository.WithdrawAsync(AccountId, 102m, "Checking", 2m, cancellationToken))
            .ReturnsAsync((100m, 2m, 98m, _createdAt));

        SavingsService service = CreateService();

        ErrorOr<(decimal AmountWithdrawn, decimal PenaltyApplied, decimal NewBalance)> result =
            await service.WithdrawAsync(TestUserId, AccountId, 100m, "Checking", cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.PenaltyApplied.Should().Be(2m);
        _savingsRepositoryMock.Verify(repository => repository.WithdrawAsync(AccountId, 102m, "Checking", 2m, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CloseAccountAsync_WhenDestinationAccountIsClosed_ShouldReturnAccountNotFound()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        SavingsAccount closing = CreateAccount(AccountId, SavingsType.HighYield);
        closing.Deposit(100m);
        SavingsAccount destination = CreateAccount(DestinationAccountId, SavingsType.HighYield);
        destination.Close();

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, true, cancellationToken))
            .ReturnsAsync([closing, destination]);

        SavingsService service = CreateService();

        ErrorOr<(decimal, decimal)> result =
            await service.CloseAccountAsync(TestUserId, AccountId, DestinationAccountId, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SavingsErrors.AccountNotFound);
    }

    [Fact]
    public async Task CloseAccountAsync_WhenFixedDepositIsClosedEarly_ShouldApplyPenalty()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        SavingsAccount closing = CreateAccount(AccountId, SavingsType.FixedDeposit, DateTime.UtcNow.AddYears(1));
        closing.Deposit(100m);
        SavingsAccount destination = CreateAccount(DestinationAccountId, SavingsType.HighYield);

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, true, cancellationToken))
            .ReturnsAsync([closing, destination]);

        _savingsRepositoryMock
            .Setup(repository => repository.CloseSavingsAccountAsync(AccountId, DestinationAccountId, 98m, 2m, cancellationToken))
            .ReturnsAsync((98m, 2m, _createdAt));

        SavingsService service = CreateService();

        ErrorOr<(decimal TransferredAmount, decimal PenaltyApplied)> result =
            await service.CloseAccountAsync(TestUserId, AccountId, DestinationAccountId, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.PenaltyApplied.Should().Be(2m);
        result.Value.TransferredAmount.Should().Be(98m);
    }

    [Fact]
    public async Task GetAccountsAsync_WhenUserHasAccounts_ShouldReturnRepositoryResult()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        SavingsAccount[] accounts = [CreateAccount(AccountId, SavingsType.HighYield)];

        _savingsRepositoryMock
            .Setup(repository => repository.GetSavingsAccountsByUserIdAsync(TestUserId, false, cancellationToken))
            .ReturnsAsync(accounts);

        SavingsService service = CreateService();

        ErrorOr<IReadOnlyCollection<SavingsAccount>> result = await service.GetAccountsAsync(TestUserId, false, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(accounts);
    }

    private SavingsService CreateService() => new(_savingsRepositoryMock.Object, _unitOfWorkMock.Object);

    private static SavingsAccount CreateAccount(int id, SavingsType savingsType, DateTime? maturityDate = null)
    {
        var account = SavingsAccount.Create(
            TestUserId,
            savingsType,
            0.02m,
            "Test account",
            null,
            null,
            null,
            maturityDate,
            _createdAt);

        typeof(SavingsAccount)
            .GetProperty(nameof(SavingsAccount.Id))!
            .SetValue(account, id);
        return account;
    }
}
