namespace BankingApp.Application.Tests;

using Application.Features.Forex.Services;
using Common.Security;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;
using Currency = NodaMoney.Currency;

/// <summary>
///     Creates commonly used mocks and domain test objects for application-layer unit tests.
/// </summary>
/// <remarks>
///     These helpers provide explicit default behavior for dependencies used by handlers and services.
///     Tests should still override or verify behavior that is important to the scenario being tested.
/// </remarks>
internal static class MockFactory
{
    private static readonly DateTime _defaultUtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    ///     Creates a user repository mock whose read operations return no users by default
    ///     and whose write operations complete successfully.
    /// </summary>
    internal static Mock<IUserRepository> CreateUserRepositoryMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IUserRepository>(behavior);

        mock.Setup(repository => repository.GetByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        mock.Setup(repository => repository.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        mock.Setup(repository => repository.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(repository => repository.UpdateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    ///     Creates an identity repository mock whose read operations return no identity account by default
    ///     and whose write operations complete successfully.
    /// </summary>
    internal static Mock<IIdentityRepository> CreateIdentityRepositoryMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IIdentityRepository>(behavior);

        mock.Setup(repository => repository.GetByUserIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);

        mock.Setup(repository => repository.GetBySessionTokenAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);

        mock.Setup(repository => repository.AddAsync(
                It.IsAny<IdentityAccount>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(repository => repository.UpdateAsync(
                It.IsAny<IdentityAccount>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    ///     Creates a hash service mock that hashes input as "hashed_{input}" and accepts all password verifications.
    /// </summary>
    /// <remarks>
    ///     Override <c>Verify</c> in tests where password validity is part of the tested behavior.
    /// </remarks>
    internal static Mock<IHashService> CreateHashServiceMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IHashService>(behavior);

        mock.Setup(service => service.GetHash(It.IsAny<string>()))
            .Returns((string input) => (ErrorOr<string>)$"hashed_{input}");

        mock.Setup(service => service.Verify(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(true);

        return mock;
    }

    /// <summary>
    ///     Creates a JWT service mock that always returns the fixed token "jwt-token".
    /// </summary>
    internal static Mock<IJsonWebTokenService> CreateJwtServiceMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IJsonWebTokenService>(behavior);

        mock.Setup(service => service.GenerateToken(It.IsAny<int>()))
            .Returns((ErrorOr<string>)"jwt-token");

        return mock;
    }

    /// <summary>
    ///     Creates a unit of work mock whose save operation completes successfully.
    /// </summary>
    internal static Mock<IUnitOfWork> CreateUnitOfWorkMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IUnitOfWork>(behavior);

        mock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    ///     Creates a system clock mock that returns a fixed UTC time.
    /// </summary>
    /// <param name="fixedTime">
    ///     The UTC time returned by the clock. If omitted, a deterministic default test time is used.
    /// </param>
    /// <param name="behavior">
    ///     Mocking behavior passed to the <see cref="Mock"/> constructor.
    /// </param>
    internal static Mock<ISystemClock> CreateSystemClockMock(
        DateTime? fixedTime = null,
        MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<ISystemClock>(behavior);

        mock.SetupGet(clock => clock.UtcNow)
            .Returns(fixedTime ?? _defaultUtcNow);

        return mock;
    }

    /// <summary>
    ///     Creates an account repository mock whose read operations return no accounts by default
    ///     and whose update operation completes successfully.
    /// </summary>
    internal static Mock<IAccountRepository> CreateAccountRepositoryMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IAccountRepository>(behavior);

        mock.Setup(repository => repository.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        mock.Setup(repository => repository.ListByUserIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Account>());

        mock.Setup(repository => repository.UpdateAsync(
                It.IsAny<Account>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(repository => repository.AddAsync(
                It.IsAny<Account>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    ///     Creates a beneficiary repository mock whose read operations return no beneficiaries by default
    ///     and whose write operations complete successfully.
    /// </summary>
    internal static Mock<IBeneficiaryRepository> CreateBeneficiaryRepositoryMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IBeneficiaryRepository>(behavior);

        mock.Setup(repository => repository.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Beneficiary?)null);

        mock.Setup(repository => repository.ListByUserIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Beneficiary>());

        mock.Setup(repository => repository.AddAsync(
                It.IsAny<Beneficiary>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(repository => repository.UpdateAsync(
                It.IsAny<Beneficiary>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(repository => repository.DeleteAsync(
                It.IsAny<Beneficiary>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    ///     Creates a transfer repository mock whose list operation returns no transfers by default
    ///     and whose add operation completes successfully.
    /// </summary>
    internal static Mock<ITransferRepository> CreateTransferRepositoryMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<ITransferRepository>(behavior);

        mock.Setup(repository => repository.ListByUserIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Transfer>());

        mock.Setup(repository => repository.AddAsync(
                It.IsAny<Transfer>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    ///     Creates a bill payment repository mock whose list operation returns no bill payments by default
    ///     and whose add operation completes successfully.
    /// </summary>
    internal static Mock<IBillPaymentRepository> CreateBillPaymentRepositoryMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IBillPaymentRepository>(behavior);

        mock.Setup(repository => repository.ListByUserIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<BillPayment>());

        mock.Setup(repository => repository.AddAsync(
                It.IsAny<BillPayment>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    ///     Creates a biller repository mock whose single-item lookup returns no biller
    ///     and whose active list operation returns an empty list.
    /// </summary>
    internal static Mock<IBillerRepository> CreateBillerRepositoryMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IBillerRepository>(behavior);

        mock.Setup(repository => repository.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Biller?)null);

        mock.Setup(repository => repository.ListActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Biller>());

        return mock;
    }

    /// <summary>
    ///     Creates a saved biller repository mock whose read operations return no saved billers by default
    ///     and whose write operations complete successfully.
    /// </summary>
    internal static Mock<ISavedBillerRepository> CreateSavedBillerRepositoryMock(
        MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<ISavedBillerRepository>(behavior);

        mock.Setup(repository => repository.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SavedBiller?)null);

        mock.Setup(repository => repository.ListByUserIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SavedBiller>());

        mock.Setup(repository => repository.AddAsync(
                It.IsAny<SavedBiller>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(repository => repository.DeleteAsync(
                It.IsAny<SavedBiller>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    ///     Creates a forex repository mock whose list operation returns no forex transactions by default
    ///     and whose add operation completes successfully.
    /// </summary>
    internal static Mock<IForexRepository> CreateForexRepositoryMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IForexRepository>(behavior);

        mock.Setup(repository => repository.ListByUserIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ForexTransaction>());

        mock.Setup(repository => repository.AddAsync(
                It.IsAny<ForexTransaction>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    ///     Creates an exchange rate service mock that always returns the fixed exchange rate 1.15.
    /// </summary>
    internal static Mock<IExchangeRateService> CreateExchangeRateServiceMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<IExchangeRateService>(behavior);

        mock.Setup(service => service.GetRate(
                It.IsAny<Currency>(),
                It.IsAny<Currency>()))
            .Returns(1.15m);

        return mock;
    }

    /// <summary>
    ///     Creates a locked rate cache mock that starts empty and allows store/remove operations.
    /// </summary>
    internal static Mock<ILockedRateCache> CreateLockedRateCacheMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<ILockedRateCache>(behavior);

        mock.Setup(cache => cache.TryGet(It.IsAny<int>()))
            .Returns((LockedRate?)null);

        mock.Setup(cache => cache.Store(
            It.IsAny<int>(),
            It.IsAny<Currency>(),
            It.IsAny<Currency>(),
            It.IsAny<decimal>(),
            It.IsAny<DateTime>()));

        mock.Setup(cache => cache.Remove(It.IsAny<int>()));

        return mock;
    }

    /// <summary>
    ///     Creates a transaction repository mock whose account transaction list operation returns an empty list.
    /// </summary>
    internal static Mock<ITransactionRepository> CreateTransactionRepositoryMock(MockBehavior behavior = MockBehavior.Strict)
    {
        var mock = new Mock<ITransactionRepository>(behavior);

        mock.Setup(repository => repository.ListByAccountIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Transaction>());

        return mock;
    }

    /// <summary>
    ///     Creates a registered user together with the matching identity account.
    /// </summary>
    /// <param name="email">
    ///     Email address used to create the user.
    /// </param>
    /// <param name="passwordHash">
    ///     Password hash wrapped in the identity account.
    /// </param>
    /// <param name="registeredAt">
    ///     Registration time. If omitted, a deterministic default UTC test time is used.
    /// </param>
    internal static (User User, IdentityAccount IdentityAccount) CreateUserWithIdentityAccount(
        string email = "test@test.com",
        string passwordHash = "test-hash",
        DateTime? registeredAt = null)
    {
        Email emailValue = Email.Create(email).Value;
        var user = User.Register(emailValue, "Test User", registeredAt ?? _defaultUtcNow);
        var identityAccount = IdentityAccount.Create(user.Id, HashedPassword.Wrap(passwordHash));

        return (user, identityAccount);
    }
}
