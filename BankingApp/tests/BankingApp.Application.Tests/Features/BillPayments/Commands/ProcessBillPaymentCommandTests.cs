namespace BankingApp.Application.Tests.Features.BillPayments.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.Features.BillPayments.Services;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.BillPaymentAggregate;
using BankingApp.Domain.Common.Errors;
using BankingApp.Domain.ReferenceData.Billers;
using Contracts.Features.BillPayments.Dtos;
using Microsoft.Extensions.Logging.Abstractions;
using NodaMoney;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;

public sealed class ProcessBillPaymentCommandTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IBillPaymentRepository> _billPaymentRepositoryMock;
    private readonly Mock<IBillerRepository> _billerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly BillPaymentService _service;

    public ProcessBillPaymentCommandTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _billPaymentRepositoryMock = new Mock<IBillPaymentRepository>();
        _billerRepositoryMock = new Mock<IBillerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        Mock<ISystemClock> clockMock = MockFactory.CreateSystemClockMock();

        _service = new BillPaymentService(
            _accountRepositoryMock.Object,
            _billPaymentRepositoryMock.Object,
            _billerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            clockMock.Object);
    }

    private static Account CreateTestAccount(int userId, AccountStatus status, Currency currency, decimal balanceAmount = 0m)
    {
        var account = Account.Open(userId, null!, currency, AccountType.Checking, null, DateTime.UtcNow);
        typeof(Account).GetProperty("Status")!.SetValue(account, status);
        typeof(Account).GetProperty("Balance")!.SetValue(account, new Money(balanceAmount, currency));
        return account;
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ShouldReturnAccountNotFoundError()
    {
        _accountRepositoryMock.Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        ErrorOr<BillPayResponse> result = await _service.ProcessAsync(1, 2, 3, "REF", 10m, TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.NotFound);

        _accountRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenAccountBelongsToDifferentUser_ShouldReturnAccountNotFoundError()
    {
        Account account = CreateTestAccount(99, AccountStatus.Active, Currency.FromCode("USD"));
        _accountRepositoryMock.Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        ErrorOr<BillPayResponse> result = await _service.ProcessAsync(1, 2, 3, "REF", 10m, TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.NotFound);

        _accountRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenAccountIsNotActive_ShouldReturnAccountNotActiveError()
    {
        Account account = CreateTestAccount(1, AccountStatus.Closed, Currency.FromCode("USD"));
        _accountRepositoryMock.Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        ErrorOr<BillPayResponse> result = await _service.ProcessAsync(1, 2, 3, "REF", 10m, TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.AccountNotActive);

        _accountRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenBillerNotFound_ShouldReturnBillerNotFoundError()
    {
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"));
        _accountRepositoryMock.Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repository => repository.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync((Biller?)null);

        ErrorOr<BillPayResponse> result = await _service.ProcessAsync(1, 2, 3, "REF", 10m, TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillPaymentErrors.NotFound);

        _accountRepositoryMock.VerifyAll();
        _billerRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenInsufficientFunds_ShouldReturnInsufficientFundsError()
    {
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"), 50m);
        var biller = new Biller { Id = 3, Name = "Test Biller" };

        _accountRepositoryMock.Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repository => repository.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(biller);

        ErrorOr<BillPayResponse> result = await _service.ProcessAsync(1, 2, 3, "REF", 100m, TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccountErrors.InsufficientFunds);

        _accountRepositoryMock.VerifyAll();
        _billerRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenAccountHasSufficientFunds_ShouldDebitAccountAndCreateBillPaymentTransaction()
    {
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"), 200m);
        var biller = new Biller { Id = 3, Name = "Test Biller" };

        _accountRepositoryMock.Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repository => repository.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(biller);

        ErrorOr<BillPayResponse> result = await _service.ProcessAsync(1, 2, 3, "REF", 100m, TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        account.Balance.Amount.Should().Be(99.50m);
        account.Transactions.Should().ContainSingle(transaction => transaction.Type == "BILL_PAYMENT" && transaction.Amount.Amount == 100m);

        _accountRepositoryMock.VerifyAll();
        _billerRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenAccountHasSufficientFunds_ShouldMarkBillPaymentAsProcessed()
    {
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"), 200m);
        var biller = new Biller { Id = 3, Name = "Test Biller" };

        _accountRepositoryMock.Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repository => repository.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(biller);

        BillPayment? savedPayment = null;
        _billPaymentRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<BillPayment>(), It.IsAny<CancellationToken>()))
            .Callback<BillPayment, CancellationToken>((payment, _) => savedPayment = payment)
            .Returns(Task.CompletedTask);

        ErrorOr<BillPayResponse> result = await _service.ProcessAsync(1, 2, 3, "REF", 50m, TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        savedPayment.Should().NotBeNull();
        savedPayment!.Status.Should().Be(BillPaymentStatus.Completed);
        savedPayment.ReceiptNumber.Should().StartWith("RCP-");
        savedPayment.LedgerTransactionId.Should().NotBeNull();

        _accountRepositoryMock.VerifyAll();
        _billerRepositoryMock.VerifyAll();
        _billPaymentRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenAccountHasSufficientFunds_ShouldSaveChanges()
    {
        Account account = CreateTestAccount(1, AccountStatus.Active, Currency.FromCode("USD"), 200m);
        var biller = new Biller { Id = 3, Name = "Test Biller" };

        _accountRepositoryMock.Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _billerRepositoryMock.Setup(repository => repository.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(biller);

        ErrorOr<BillPayResponse> result = await _service.ProcessAsync(1, 2, 3, "REF", 50m, TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _accountRepositoryMock.VerifyAll();
        _billerRepositoryMock.VerifyAll();
    }
}
