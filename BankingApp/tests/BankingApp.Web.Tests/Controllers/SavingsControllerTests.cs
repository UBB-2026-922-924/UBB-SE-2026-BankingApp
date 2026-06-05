namespace BankingApp.Web.Tests.Controllers;

using BankingApp.Infrastructure.Http.Features.Savings.Services;
using BankingApp.Web.Controllers;
using BankingApp.Web.Models.Savings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class SavingsControllerTests : IDisposable
{
    private readonly Mock<ISavingsRepoProxy> _savingsRepoProxy = new(MockBehavior.Loose);
    private readonly Mock<ISavingsUiRulesRepoProxy> _uiRulesProxy = new(MockBehavior.Loose);
    private readonly Mock<ISavingsWorkflowRepoProxy> _workflowProxy = new(MockBehavior.Loose);
    private readonly Mock<ISavingsPresentationRepoProxy> _presentationProxy = new(MockBehavior.Loose);
    private readonly SavingsController _controller;

    public SavingsControllerTests()
    {
        DefaultHttpContext httpContext = new();
        _controller = new SavingsController(
            _savingsRepoProxy.Object,
            _uiRulesProxy.Object,
            _workflowProxy.Object,
            _presentationProxy.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public async Task Index_WhenProxyThrows_ShouldReturnFallbackViewInsteadOfCrashing()
    {
        _savingsRepoProxy
            .Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        IActionResult result = await _controller.Index();

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<SavingsPageViewModel>();
        _controller.TempData["Error"].Should().NotBeNull();
    }

    [Fact]
    public async Task Index_WhenProxyThrows_ShouldReturnFallbackModelWithNonNullCollections()
    {
        _savingsRepoProxy
            .Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ThrowsAsync(new HttpRequestException("timeout"));

        IActionResult result = await _controller.Index();

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        SavingsPageViewModel model = viewResult.Model.Should().BeOfType<SavingsPageViewModel>().Subject;
        model.Accounts.Should().NotBeNull();
        model.FundingSources.Should().NotBeNull();
        model.CloseDestinationAccounts.Should().NotBeNull();
    }

    [Fact]
    public async Task Index_WhenProxyThrows_ShouldSetErrorInTempData()
    {
        const string errorMessage = "upstream failure";
        _savingsRepoProxy
            .Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        await _controller.Index();

        _controller.TempData["Error"].Should().Be(errorMessage);
    }

    [Fact]
    public async Task Index_WhenPresentationProxyThrows_ShouldReturnFallbackView()
    {
        _savingsRepoProxy
            .Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync([]);
        _savingsRepoProxy
            .Setup(proxy => proxy.GetFundingSourcesAsync(It.IsAny<int>()))
            .ReturnsAsync([]);
        _presentationProxy
            .Setup(proxy => proxy.GetTotalSavedAmount(It.IsAny<IEnumerable<Domain.Aggregates.SavingsAggregate.SavingsAccount>>()))
            .ThrowsAsync(new HttpRequestException("presentation service down"));

        IActionResult result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
        _controller.TempData["Error"].Should().NotBeNull();
    }
}
