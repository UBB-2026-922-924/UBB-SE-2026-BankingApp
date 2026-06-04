namespace BankingApp.Web.Tests.Controllers;

using Contracts.Features.Billers.Dtos;
using Contracts.Features.Billers.Services;
using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.BillPayments.Services;
using BankingApp.Web.Controllers;
using BankingApp.Web.Models.BillPayments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class BillPaymentsControllerTests : IDisposable
{
    private readonly Mock<IBillPaymentService> _billPaymentServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IBillerService> _billerServiceMock = new(MockBehavior.Strict);
    private readonly BillPaymentsController _controller;

    public BillPaymentsControllerTests()
    {
        DefaultHttpContext httpContext = new();

        _controller = new BillPaymentsController(
            _billPaymentServiceMock.Object,
            _billerServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public async Task Index_WhenAllServicesSucceed_ShouldReturnViewWithPopulatedModel()
    {
        // Arrange
        List<SavedBillerDto> savedBillers =
        [
            new SavedBillerDto
            {
                Id = 1,
                BillerId = 10,
                BillerName = "Electric Company",
                Nickname = "My Electric",
                DefaultReference = "REF-001"
            }
        ];

        List<BillerDto> allBillers =
        [
            new BillerDto
            {
                Id = 10,
                Name = "Electric Company",
                Category = "Utilities",
                IsActive = true
            },
            new BillerDto
            {
                Id = 20,
                Name = "Water Services",
                Category = "Utilities",
                IsActive = true
            }
        ];

        List<AccountDto> accounts =
        [
            new AccountDto
            {
                Id = 100,
                Iban = "RO49BANK1234567890",
                Currency = "RON",
                Balance = 5000.00m,
                AccountName = "Current Account"
            }
        ];

        _billerServiceMock
            .Setup(service => service.GetSavedBillersAsync(CancellationToken.None))
            .ReturnsAsync(savedBillers);

        _billerServiceMock
            .Setup(service => service.GetBillersAsync(null, null, CancellationToken.None))
            .ReturnsAsync(allBillers);

        _billPaymentServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(accounts);

        // Act
        IActionResult result = await _controller.Index(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        BillPayModel viewModel = viewResult.Model.Should().BeOfType<BillPayModel>().Subject;

        viewModel.SavedBillers.Should().HaveCount(1);
        viewModel.SavedBillers[0].BillerName.Should().Be("Electric Company");
        viewModel.SavedBillers[0].Nickname.Should().Be("My Electric");

        viewModel.AllBillers.Should().HaveCount(2);
        viewModel.AllBillers[0].Name.Should().Be("Electric Company");
        viewModel.AllBillers[1].Name.Should().Be("Water Services");

        viewModel.Accounts.Should().HaveCount(1);
        viewModel.Accounts[0].Iban.Should().Be("RO49BANK1234567890");
        viewModel.Accounts[0].Balance.Should().Be(5000.00m);

        _billerServiceMock.VerifyAll();
        _billPaymentServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Index_WhenGetBillersFails_ShouldSetTempDataErrorAndReturnEmptyModel()
    {
        // Arrange
        _billerServiceMock
            .Setup(service => service.GetSavedBillersAsync(CancellationToken.None))
            .ReturnsAsync(new List<SavedBillerDto>());

        _billerServiceMock
            .Setup(service => service.GetBillersAsync(null, null, CancellationToken.None))
            .ReturnsAsync(Error.Failure("billers.load_failed", "Unable to load billers."));

        _billPaymentServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(new List<AccountDto>());

        // Act
        IActionResult result = await _controller.Index(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        BillPayModel viewModel = viewResult.Model.Should().BeOfType<BillPayModel>().Subject;

        viewModel.SavedBillers.Should().BeEmpty();
        viewModel.AllBillers.Should().BeEmpty();
        viewModel.Accounts.Should().BeEmpty();
        _controller.TempData["Error"].Should().Be("Unable to load billers. Please try again.");

        _billerServiceMock.VerifyAll();
        _billPaymentServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Index_WhenGetAccountsFails_ShouldSetTempDataErrorAndReturnEmptyModel()
    {
        // Arrange
        _billerServiceMock
            .Setup(service => service.GetSavedBillersAsync(CancellationToken.None))
            .ReturnsAsync(new List<SavedBillerDto>());

        _billerServiceMock
            .Setup(service => service.GetBillersAsync(null, null, CancellationToken.None))
            .ReturnsAsync(new List<BillerDto>());

        _billPaymentServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("accounts.load_failed", "Unable to load accounts."));

        // Act
        IActionResult result = await _controller.Index(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        BillPayModel viewModel = viewResult.Model.Should().BeOfType<BillPayModel>().Subject;

        viewModel.SavedBillers.Should().BeEmpty();
        viewModel.AllBillers.Should().BeEmpty();
        viewModel.Accounts.Should().BeEmpty();
        _controller.TempData["Error"].Should().Be("Unable to load your accounts. Please try again.");

        _billerServiceMock.VerifyAll();
        _billPaymentServiceMock.VerifyAll();
    }
}
