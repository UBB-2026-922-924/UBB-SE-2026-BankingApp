namespace BankingApp.Web.Tests.Controllers;

using BankingApp.Infrastructure.Http.Features.Loans.Services;
using BankingApp.Web.Controllers;
using BankingApp.Web.Models.Loans;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class LoansControllerTests : IDisposable
{
    private readonly Mock<ILoansRepoProxy> _loansRepoProxy = new(MockBehavior.Loose);
    private readonly Mock<ILoanDialogStateRepoProxy> _dialogStateProxy = new(MockBehavior.Loose);
    private readonly Mock<ILoanApplicationPresentationRepoProxy> _applicationPresentationProxy = new(MockBehavior.Loose);
    private readonly LoansController _controller;

    public LoansControllerTests()
    {
        DefaultHttpContext httpContext = new();
        _controller = new LoansController(
            _loansRepoProxy.Object,
            _dialogStateProxy.Object,
            _applicationPresentationProxy.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public async Task Index_WhenProxyThrows_ReturnsFallbackViewInsteadOfCrashing()
    {
        _loansRepoProxy
            .Setup(proxy => proxy.GetLoansByUserAsync(It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        IActionResult result = await _controller.Index();

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<LoansPageViewModel>();
        _controller.TempData["Error"].Should().NotBeNull();
    }

    [Fact]
    public async Task Index_WhenProxyThrows_FallbackModelHasNonNullCollections()
    {
        _loansRepoProxy
            .Setup(proxy => proxy.GetLoansByUserAsync(It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("timeout"));

        IActionResult result = await _controller.Index();

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        LoansPageViewModel model = viewResult.Model.Should().BeOfType<LoansPageViewModel>().Subject;
        model.Loans.Should().NotBeNull();
        model.Application.Should().NotBeNull();
    }

    [Fact]
    public async Task Index_WhenProxyThrows_SetsErrorInTempData()
    {
        const string errorMessage = "loan service down";
        _loansRepoProxy
            .Setup(proxy => proxy.GetLoansByUserAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        await _controller.Index();

        _controller.TempData["Error"].Should().Be(errorMessage);
    }

    [Fact]
    public async Task Index_WhenProxyReturnsEmptyList_ReturnsViewWithNoLoans()
    {
        _loansRepoProxy
            .Setup(proxy => proxy.GetLoansByUserAsync(It.IsAny<int>()))
            .ReturnsAsync([]);

        IActionResult result = await _controller.Index();

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        LoansPageViewModel model = viewResult.Model.Should().BeOfType<LoansPageViewModel>().Subject;
        model.HasLoans.Should().BeFalse();
        model.Loans.Should().BeEmpty();
    }
}
