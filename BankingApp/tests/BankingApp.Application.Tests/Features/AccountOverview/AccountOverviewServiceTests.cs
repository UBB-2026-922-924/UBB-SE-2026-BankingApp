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
using NUnit.Framework;
using NUnit.Framework.Internal;
using CardType = Domain.Enums.CardType;
using Currency = NodaMoney.Currency;
using Transaction = Domain.Aggregates.AccountAggregate.Entities.Transaction;

[TestFixture]
public class AccountOverviewServiceTests
{
    private IUserRepository mockUserRepository = Substitute.For<IUserRepository>();
    private IAccountRepository mockAccountRepository = Substitute.For<IAccountRepository>();
    private ITransactionRepository mockTransactionRepository = Substitute.For<ITransactionRepository>();

    private AccountOverviewService accountOverviewService;

    [SetUp]
    public void SetUp()
    {
        mockUserRepository = Substitute.For<IUserRepository>();
        mockAccountRepository = Substitute.For<IAccountRepository>();
        mockTransactionRepository = Substitute.For<ITransactionRepository>();

        accountOverviewService = new AccountOverviewService(
            mockUserRepository,
            mockAccountRepository,
            mockTransactionRepository,
            NullLogger< AccountOverviewService>.Instance);
    }

    [Test]
    public void GetDashboard_WhenUserIsNull_ReturnsNotFoundError()
    {
        mockUserRepository.GetByIdAsync(1).Returns((User)null!);

        ErrorOr<AccountOverviewDto> getDashboardResponse = accountOverviewService.GetDashboardAsync(1).Result;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(getDashboardResponse.IsError, Is.True);
            Assert.That(getDashboardResponse.FirstError, Is.EqualTo(UserErrors.NotFound));
        }
    }

    [Test]
    public void GetDashboard_WhenUserExists_ReturnsTransactions()
    {

        var user = User.Register(Email.Create("example@example.com").Value, "full", DateTime.Today);
        mockUserRepository.GetByIdAsync(1).Returns(user);

        var account = Account.Open(0, Iban.Create("RO20GJSDGNSDJAFFBDGJSDGBGFJDFFG").Value, new Currency(),
            AccountType.Checking, "name", new DateTime());
     
        var accounts = new ReadOnlyCollection<Account>(new List<Account>([account]));

        account.IssueCard("aaa", "aaaa", new DateTime(2028, 1, 1), "222", CardType.Debit, "brand", new DateTime());

        mockAccountRepository.ListByUserIdAsync(0).Returns(accounts);

        var transaction = Domain.Aggregates.AccountAggregate.Entities.Transaction.Create(0, "dhdfsd", "dggf",
            TransactionDirection.In, new Money(10), new Money(20), TransactionStatus.Completed, new DateTime());
        mockTransactionRepository.ListByAccountIdAsync(0).Returns(new ReadOnlyCollection<Domain.Aggregates.AccountAggregate.Entities.Transaction>(new List<Transaction>([transaction])));

        ErrorOr<AccountOverviewDto> getDashboardResponse = accountOverviewService.GetDashboardAsync(1).Result;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(getDashboardResponse.IsSuccess, Is.True);
            Assert.That(getDashboardResponse.Value.Cards.Count(f => true), Is.GreaterThan(0));
            Assert.That(getDashboardResponse.Value.RecentTransactions.Count(f => true), Is.GreaterThan(0));
        }
    }
}
