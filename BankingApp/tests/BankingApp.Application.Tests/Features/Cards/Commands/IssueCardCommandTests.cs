namespace BankingApp.Application.Tests.Features.Cards.Commands;

using BankingApp.Application.Features.Cards.Services;
using Contracts.Features.Cards.Dtos;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.ValueObjects;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Clock;
using Shared.Persistence;

public sealed class IssueCardCommandTests
{
    private const int SaveChangesCallsPerIssuance = 2;

    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISystemClock> _clockMock;
    private readonly CardService _service;

    public IssueCardCommandTests()
    {
        _accountRepositoryMock = MockFactory.CreateAccountRepositoryMock();
        _userRepositoryMock = MockFactory.CreateUserRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _clockMock = MockFactory.CreateSystemClockMock();
        _service = new CardService(
            _accountRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            NullLogger<CardService>.Instance);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        ErrorOr<CardDetailsDto> result = await _service.IssueAsync(1, CardType.Debit, "Visa", TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldCreateAccountAndIssueCard()
    {
        Email email = Email.Create("john@example.com").Value;
        var user = User.Register(email, "John Doe", DateTime.UtcNow);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        ErrorOr<CardDetailsDto> result = await _service.IssueAsync(1, CardType.Debit, "Visa", TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<CardDetailsDto>();
        result.Value.CardholderName.Should().Be("JOHN DOE");
        result.Value.CardType.Should().Be(CardType.Debit);
        result.Value.CardBrand.Should().Be("Visa");
        result.Value.Status.Should().Be(CardStatus.Active);
        _accountRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(SaveChangesCallsPerIssuance));
    }

    [Fact]
    public async Task Handle_WhenCreditCard_ShouldCreateCreditAccount()
    {
        Email email = Email.Create("jane@example.com").Value;
        var user = User.Register(email, "Jane Doe", DateTime.UtcNow);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _accountRepositoryMock
            .Setup(r => r.AddAsync(It.Is<Account>(a => a.AccountType == AccountType.Credit), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        ErrorOr<CardDetailsDto> result = await _service.IssueAsync(1, CardType.Credit, "Mastercard", TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.CardType.Should().Be(CardType.Credit);
        _accountRepositoryMock.Verify(r => r.AddAsync(It.Is<Account>(a => a.AccountType == AccountType.Credit), It.IsAny<CancellationToken>()), Times.Once);
    }
}
