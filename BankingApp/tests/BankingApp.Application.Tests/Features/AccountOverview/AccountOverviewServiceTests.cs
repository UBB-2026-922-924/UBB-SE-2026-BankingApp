namespace BankingApp.Application.Tests.Features.AccountOverview;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Application.Features.AccountOverview.Services;
using BankingApp.Application.Common.Security;
using BankingApp.Application.Features.Cards.Services;
using BankingApp.Contracts.Features.AccountOverview.Dtos;
using Bogus.DataSets;
using Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NodaMoney;
using NSubstitute;
using CardType = Domain.Enums.CardType;
using Currency = NodaMoney.Currency;
using Transaction = Domain.Aggregates.AccountAggregate.Entities.Transaction;
public sealed class AccountOverviewServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;

    private readonly AccountOverviewService _accountOverviewService;
    public AccountOverviewServiceTests()
    {
        _mockUserRepository = MockFactory.CreateUserRepositoryMock();
        _mockAccountRepository = MockFactory.CreateAccountRepositoryMock();
        _mockTransactionRepository = MockFactory.CreateTransactionRepositoryMock();

        _accountOverviewService = new AccountOverviewService(
            _mockUserRepository.Object,
            _mockAccountRepository.Object,
            _mockTransactionRepository.Object,
            NullLogger< AccountOverviewService>.Instance);
    }

    [Fact]
    public async Task GetDashboard_WhenUserIsNull_ReturnsNotFoundError()
    {
        _mockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        ErrorOr<AccountOverviewDto> getDashboardResponse = await _accountOverviewService.GetDashboardAsync(1, TestContext.Current.CancellationToken);

        getDashboardResponse.IsError.Should().BeTrue();
        getDashboardResponse.FirstError.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task GetDashboard_WhenUserExists_ReturnsTransactions()
    {

        var user = User.Register(Email.Create("example@example.com").Value, "full", DateTime.Today);
        _mockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var account = Account.Open(0, Iban.Create("RO12BANK1234567890123456").Value, new Currency(),
            AccountType.Checking, "name", new DateTime());

        account.IssueCard("aaa", "aaaa", new DateTime(2028, 1, 1), "222", CardType.Debit, "brand", new DateTime());

        var accounts = new ReadOnlyCollection<Account>(new List<Account>([account]));

        _mockAccountRepository.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        var transaction = Domain.Aggregates.AccountAggregate.Entities.Transaction.Create(0, "dhdfsd", "dggf",
            TransactionDirection.In, new Money(10), new Money(20), TransactionStatus.Completed, new DateTime());
        _mockTransactionRepository.Setup(r => r.ListByAccountIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadOnlyCollection<Domain.Aggregates.AccountAggregate.Entities.Transaction>(new List<Transaction>([transaction])));

        ErrorOr<AccountOverviewDto> getDashboardResponse = await _accountOverviewService.GetDashboardAsync(1, TestContext.Current.CancellationToken);

        getDashboardResponse.IsSuccess.Should().BeTrue();
        getDashboardResponse.Value.Cards.Should().HaveCountGreaterThan(0);
        getDashboardResponse.Value.RecentTransactions.Should().HaveCountGreaterThan(0);
    }
}
