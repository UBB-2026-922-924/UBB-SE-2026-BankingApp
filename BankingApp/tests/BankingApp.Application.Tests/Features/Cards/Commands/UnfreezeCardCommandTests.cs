namespace BankingApp.Application.Tests.Features.Cards.Commands;

using BankingApp.Application.Features.Cards.Services;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using NodaMoney;
using Shared.Persistence;

public sealed class UnfreezeCardCommandTests
{
    private const string TestCardNumber = "1234567890123456";
    private const string TestCvv = "123";
    private const string TestCardholderName = "John Doe";
    private const string TestCurrency = "USD";
    private const string TestCardBrand = "Visa";
    private const int CardExpiryYears = 2;

    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CardService _service;

    public UnfreezeCardCommandTests()
    {
        _accountRepositoryMock = MockFactory.CreateAccountRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        Mock<Shared.Clock.ISystemClock> clockMock = MockFactory.CreateSystemClockMock();
        Mock<IUserRepository> userRepositoryMock = MockFactory.CreateUserRepositoryMock();
        _service = new CardService(
            _accountRepositoryMock.Object,
            userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            clockMock.Object,
            NullLogger<CardService>.Instance);
    }

    [Fact]
    public async Task Handle_WhenCardNotFound_ShouldReturnNotFoundError()
    {
        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Account>());

        ErrorOr<Success> result = await _service.UnfreezeAsync(1, 99, TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CardErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCardIsCancelled_ShouldReturnAlreadyCancelledError()
    {
        var account = Account.Open(1, null!, Currency.FromCode(TestCurrency), AccountType.Checking, null, DateTime.UtcNow);
        Card card = account.IssueCard(TestCardNumber, TestCardholderName, DateTime.UtcNow.AddYears(CardExpiryYears), TestCvv, CardType.Debit, TestCardBrand, DateTime.UtcNow);
        card.Cancel(DateTime.UtcNow);

        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        ErrorOr<Success> result = await _service.UnfreezeAsync(1, card.Id, TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CardErrors.AlreadyCancelled);
    }

    [Fact]
    public async Task Handle_WhenCardIsNotFrozen_ShouldReturnNotFrozenError()
    {
        var account = Account.Open(1, null!, Currency.FromCode(TestCurrency), AccountType.Checking, null, DateTime.UtcNow);
        Card card = account.IssueCard(TestCardNumber, TestCardholderName, DateTime.UtcNow.AddYears(CardExpiryYears), TestCvv, CardType.Debit, TestCardBrand, DateTime.UtcNow);

        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        ErrorOr<Success> result = await _service.UnfreezeAsync(1, card.Id, TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CardErrors.NotFrozen);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUnfreezeCardAndSaveChanges()
    {
        var account = Account.Open(1, null!, Currency.FromCode(TestCurrency), AccountType.Checking, null, DateTime.UtcNow);
        Card card = account.IssueCard(TestCardNumber, TestCardholderName, DateTime.UtcNow.AddYears(CardExpiryYears), TestCvv, CardType.Debit, TestCardBrand, DateTime.UtcNow);
        card.Freeze();

        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        ErrorOr<Success> result = await _service.UnfreezeAsync(1, card.Id, TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        card.Status.Should().Be(CardStatus.Active);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
