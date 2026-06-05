namespace BankingApp.Web.Tests.Controllers;

using BankingApp.Contracts.Features.Chat.Dtos;
using BankingApp.Infrastructure.Http.Features.Chat.Services;
using BankingApp.Web.Controllers;
using BankingApp.Web.Models.Chat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class ChatControllerTests : IDisposable
{
    private readonly Mock<IChatRepoProxy> _chatRepoProxy = new(MockBehavior.Strict);
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        DefaultHttpContext httpContext = new();
        _controller = new ChatController(_chatRepoProxy.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public async Task Index_WhenProxyReturnsSessions_ShouldReturnViewWithSessions()
    {
        List<ChatSessionDto> sessions =
        [
            new ChatSessionDto { Id = 1, Subject = "Billing query", Status = "Open" },
            new ChatSessionDto { Id = 2, Subject = "Card issue", Status = "Closed" },
        ];
        _chatRepoProxy
            .Setup(proxy => proxy.GetSessionsAsync())
            .ReturnsAsync(sessions);

        IActionResult result = await _controller.Index();

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        ChatIndexViewModel model = viewResult.Model.Should().BeOfType<ChatIndexViewModel>().Subject;
        model.Sessions.Should().HaveCount(2);
        model.ErrorMessage.Should().BeNull();
        _chatRepoProxy.VerifyAll();
    }

    [Fact]
    public async Task Index_WhenProxyThrows_ShouldReturnFallbackViewWithErrorMessage()
    {
        _chatRepoProxy
            .Setup(proxy => proxy.GetSessionsAsync())
            .ThrowsAsync(new HttpRequestException("Chat service unavailable"));

        IActionResult result = await _controller.Index();

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        ChatIndexViewModel model = viewResult.Model.Should().BeOfType<ChatIndexViewModel>().Subject;
        model.Sessions.Should().BeEmpty();
        model.ErrorMessage.Should().Be("Chat service unavailable");
        _chatRepoProxy.VerifyAll();
    }

    [Fact]
    public async Task Details_WhenSessionNotFound_ShouldReturnNotFound()
    {
        _chatRepoProxy
            .Setup(proxy => proxy.GetSessionAsync(99))
            .ThrowsAsync(new HttpRequestException("Not Found", null, System.Net.HttpStatusCode.NotFound));

        IActionResult result = await _controller.Details(99);

        result.Should().BeOfType<NotFoundResult>();
        _chatRepoProxy.VerifyAll();
    }

    [Fact]
    public async Task Details_WhenSessionFound_ShouldReturnViewWithMessages()
    {
        ChatSessionDto session = new()
        {
            Id = 5,
            Subject = "Help needed",
            Status = "Open",
            Messages = [new ChatMessageDto { Id = 1, Content = "Hello", Sender = "Customer" }],
        };
        _chatRepoProxy
            .Setup(proxy => proxy.GetSessionAsync(5))
            .ReturnsAsync(session);

        IActionResult result = await _controller.Details(5);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        ChatDetailsViewModel model = viewResult.Model.Should().BeOfType<ChatDetailsViewModel>().Subject;
        model.Session.Should().Be(session);
        model.Messages.Should().ContainSingle();
        _chatRepoProxy.VerifyAll();
    }

    [Fact]
    public async Task Create_WhenProxySucceeds_ShouldRedirectToDetails()
    {
        ChatSessionDto newSession = new() { Id = 10, Subject = "General", Status = "Open" };
        _chatRepoProxy
            .Setup(proxy => proxy.CreateSessionAsync("General"))
            .ReturnsAsync(newSession);

        IActionResult result = await _controller.Create("General");

        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.RouteValues!["id"].Should().Be(10);
        _chatRepoProxy.VerifyAll();
    }

    [Fact]
    public async Task Create_WhenProxyThrows_ShouldRedirectToIndexWithTempDataError()
    {
        _chatRepoProxy
            .Setup(proxy => proxy.CreateSessionAsync(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Could not open session"));

        IActionResult result = await _controller.Create("Billing");

        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["Error"].Should().Be("Could not open session");
        _chatRepoProxy.VerifyAll();
    }
}
