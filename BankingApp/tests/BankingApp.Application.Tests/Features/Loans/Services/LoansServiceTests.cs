namespace BankingApp.Application.Tests.Features.Loans.Services;

using Application.Features.Loans.Services;
using Contracts.Features.Loans.Dtos;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;
using BankingApp.Domain.Common.Errors;
using ErrorOr;

public sealed class LoansServiceTests
{
    private const int TestUserId = 1;
    private const int LoanId = 10;

    private readonly Mock<ILoanRepository> _loanRepositoryMock = new(MockBehavior.Strict);

    [Fact]
    public async Task PayInstallmentAsync_WhenLoanDoesNotExist_ShouldReturnLoanNotFound()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _loanRepositoryMock
            .Setup(repository => repository.GetLoanByIdAsync(LoanId, cancellationToken))
            .ReturnsAsync((Loan?)null);

        LoansService service = CreateService();

        ErrorOr<Success> result = await service.PayInstallmentAsync(LoanId, null, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(LoanErrors.LoanNotFound);
    }

    [Fact]
    public async Task PayInstallmentAsync_WhenLoanIsAlreadyPassed_ShouldReturnLoanAlreadyClosed()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Loan loan = CreateLoan(LoanStatus.Passed, 0, 0m, 100m);

        _loanRepositoryMock
            .Setup(repository => repository.GetLoanByIdAsync(LoanId, cancellationToken))
            .ReturnsAsync(loan);

        LoansService service = CreateService();

        ErrorOr<Success> result = await service.PayInstallmentAsync(LoanId, null, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(LoanErrors.LoanAlreadyClosed);
    }

    [Fact]
    public async Task PayInstallmentAsync_WhenPaymentAmountIsNegative_ShouldReturnInvalidPaymentAmount()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Loan loan = CreateLoan(LoanStatus.Active, 12, 1000m, 100m);

        _loanRepositoryMock
            .Setup(repository => repository.GetLoanByIdAsync(LoanId, cancellationToken))
            .ReturnsAsync(loan);

        LoansService service = CreateService();

        ErrorOr<Success> result = await service.PayInstallmentAsync(LoanId, -50m, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(LoanErrors.InvalidPaymentAmount);
    }

    [Fact]
    public async Task PayInstallmentAsync_WhenStandardPayment_ShouldUpdateLoanInRepository()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Loan loan = CreateLoan(LoanStatus.Active, 12, 1000m, 100m);

        _loanRepositoryMock
            .Setup(repository => repository.GetLoanByIdAsync(LoanId, cancellationToken))
            .ReturnsAsync(loan);

        _loanRepositoryMock
            .Setup(repository => repository.UpdateLoanAfterPaymentAsync(LoanId, 900m, 11, LoanStatus.Active, cancellationToken))
            .Returns(Task.CompletedTask);

        LoansService service = CreateService();

        ErrorOr<Success> result = await service.PayInstallmentAsync(LoanId, null, cancellationToken);

        result.IsError.Should().BeFalse();
        _loanRepositoryMock.Verify(repository => repository.UpdateLoanAfterPaymentAsync(LoanId, 900m, 11, LoanStatus.Active, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task PayInstallmentAsync_WhenCustomPaymentPaysOffLoan_ShouldMarkLoanAsPassed()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Loan loan = CreateLoan(LoanStatus.Active, 1, 90m, 100m);

        _loanRepositoryMock
            .Setup(repository => repository.GetLoanByIdAsync(LoanId, cancellationToken))
            .ReturnsAsync(loan);

        _loanRepositoryMock
            .Setup(repository => repository.UpdateLoanAfterPaymentAsync(LoanId, 0m, 0, LoanStatus.Passed, cancellationToken))
            .Returns(Task.CompletedTask);

        LoansService service = CreateService();

        ErrorOr<Success> result = await service.PayInstallmentAsync(LoanId, 90m, cancellationToken);

        result.IsError.Should().BeFalse();
        _loanRepositoryMock.Verify(repository => repository.UpdateLoanAfterPaymentAsync(LoanId, 0m, 0, LoanStatus.Passed, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetLoansByUserAsync_WhenUserHasLoans_ShouldReturnRepositoryResult()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Loan[] expectedLoans =
        [
            CreateLoan(LoanStatus.Active, 12, 1000m, 100m),
            CreateLoan(LoanStatus.Passed, 0, 0m, 100m),
        ];

        _loanRepositoryMock
            .Setup(repository => repository.GetLoansByUserAsync(TestUserId, cancellationToken))
            .ReturnsAsync(expectedLoans);

        LoansService service = CreateService();

        ErrorOr<IReadOnlyCollection<Loan>> result = await service.GetLoansByUserAsync(TestUserId, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedLoans);
    }

    [Theory]
    [InlineData(LoanType.Personal, 8.5)]
    [InlineData(LoanType.Mortgage, 4.5)]
    [InlineData(LoanType.Student, 3.0)]
    [InlineData(LoanType.Auto, 6.5)]
    public void GetEstimate_WhenLoanTypeIsValid_ShouldReturnExpectedInterestRate(LoanType loanType, decimal expectedRate)
    {
        LoanApplicationRequest request = CreateApplicationRequest(loanType);

        LoansService service = CreateService();

        ErrorOr<LoanEstimate> result = service.GetEstimate(request);

        result.IsError.Should().BeFalse();
        result.Value.IndicativeRate.Should().Be(expectedRate);
        result.Value.MonthlyInstallment.Should().BeGreaterThan(0m);
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenUserHasMaximumActiveLoans_ShouldReturnRejectedResult()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        LoanApplicationRequest request = CreateApplicationRequest(LoanType.Personal);
        Loan[] existingLoans = Enumerable.Repeat(CreateLoan(LoanStatus.Active, 12, 1000m, 100m), 5).ToArray();

        _loanRepositoryMock
            .Setup(repository => repository.CreateLoanApplicationAsync(It.IsAny<LoanApplication>(), cancellationToken))
            .ReturnsAsync(10);

        _loanRepositoryMock
            .Setup(repository => repository.GetLoansByUserAsync(request.UserId, cancellationToken))
            .ReturnsAsync(existingLoans);

        _loanRepositoryMock
            .Setup(repository => repository.UpdateLoanApplicationStatusAsync(10, LoanApplicationStatus.Rejected, It.IsAny<string>(), cancellationToken))
            .Returns(Task.CompletedTask);

        LoansService service = CreateService();

        ErrorOr<LoanApplicationResult> result = await service.SubmitApplicationAsync(request, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be(LoanApplicationStatus.Rejected.ToString());
        result.Value.RejectionReason.Should().Be("Maximum number of active loans reached.");
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenTotalDebtLimitReached_ShouldReturnRejectedResult()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        LoanApplicationRequest request = CreateApplicationRequest(LoanType.Personal);
        Loan[] existingLoans = [CreateLoan(LoanStatus.Active, 12, 199000m, 100m)];

        _loanRepositoryMock
            .Setup(repository => repository.CreateLoanApplicationAsync(It.IsAny<LoanApplication>(), cancellationToken))
            .ReturnsAsync(10);

        _loanRepositoryMock
            .Setup(repository => repository.GetLoansByUserAsync(request.UserId, cancellationToken))
            .ReturnsAsync(existingLoans);

        _loanRepositoryMock
            .Setup(repository => repository.UpdateLoanApplicationStatusAsync(10, LoanApplicationStatus.Rejected, It.IsAny<string>(), cancellationToken))
            .Returns(Task.CompletedTask);

        LoansService service = CreateService();

        ErrorOr<LoanApplicationResult> result = await service.SubmitApplicationAsync(request, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be(LoanApplicationStatus.Rejected.ToString());
        result.Value.RejectionReason.Should().Be("Total debt limit exceeded.");
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenApplicationIsApproved_ShouldCreateLoanAndAmortization()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        LoanApplicationRequest request = CreateApplicationRequest(LoanType.Personal);

        _loanRepositoryMock
            .Setup(repository => repository.CreateLoanApplicationAsync(It.IsAny<LoanApplication>(), cancellationToken))
            .ReturnsAsync(10);

        _loanRepositoryMock
            .Setup(repository => repository.GetLoansByUserAsync(request.UserId, cancellationToken))
            .ReturnsAsync(Array.Empty<Loan>());

        _loanRepositoryMock
            .Setup(repository => repository.UpdateLoanApplicationStatusAsync(10, LoanApplicationStatus.Approved, null, cancellationToken))
            .Returns(Task.CompletedTask);

        _loanRepositoryMock
            .Setup(repository => repository.CreateLoanAsync(It.IsAny<Loan>(), cancellationToken))
            .ReturnsAsync(LoanId);

        _loanRepositoryMock
            .Setup(repository => repository.SaveAmortizationAsync(It.IsAny<IReadOnlyCollection<AmortizationRow>>(), cancellationToken))
            .Returns(Task.CompletedTask);

        LoansService service = CreateService();

        ErrorOr<LoanApplicationResult> result = await service.SubmitApplicationAsync(request, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be(LoanApplicationStatus.Approved.ToString());
        _loanRepositoryMock.Verify(repository => repository.CreateLoanAsync(It.Is<Loan>(loan =>
            loan.UserId == request.UserId &&
            loan.LoanType == request.LoanType &&
            loan.Principal == request.DesiredAmount), cancellationToken), Times.Once);
        _loanRepositoryMock.Verify(repository => repository.SaveAmortizationAsync(It.IsAny<IReadOnlyCollection<AmortizationRow>>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAmortizationAsync_WhenRowsExist_ShouldReturnExistingSchedule()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Loan loan = CreateLoan(LoanStatus.Active, 12, 1000m, 100m);
        AmortizationRow[] rows =
        [
            AmortizationRow.Create(LoanId, 1, DateTime.Today.AddMonths(1), 80m, 20m, 9920m),
        ];

        _loanRepositoryMock
            .Setup(repository => repository.GetLoanByIdAsync(LoanId, cancellationToken))
            .ReturnsAsync(loan);

        _loanRepositoryMock
            .Setup(repository => repository.GetAmortizationAsync(LoanId, cancellationToken))
            .ReturnsAsync(rows);

        LoansService service = CreateService();

        ErrorOr<IReadOnlyCollection<AmortizationRow>> result = await service.GetAmortizationAsync(LoanId, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(rows);
        _loanRepositoryMock.Verify(repository => repository.SaveAmortizationAsync(It.IsAny<IReadOnlyCollection<AmortizationRow>>(), cancellationToken), Times.Never);
    }

    private LoansService CreateService() => new(_loanRepositoryMock.Object);

    private static LoanApplicationRequest CreateApplicationRequest(LoanType loanType) =>
        new()
        {
            UserId = TestUserId,
            LoanType = loanType,
            DesiredAmount = 12000m,
            PreferredTermMonths = 12,
            Purpose = "Personal expenses",
        };

    private static Loan CreateLoan(LoanStatus status, int remainingMonths, decimal outstandingBalance, decimal monthlyInstallment) =>
        Loan.Reconstitute(
            LoanId,
            TestUserId,
            LoanType.Personal,
            12000m,
            outstandingBalance,
            8.5m,
            monthlyInstallment,
            remainingMonths,
            status,
            12,
            DateTime.UtcNow);
}
