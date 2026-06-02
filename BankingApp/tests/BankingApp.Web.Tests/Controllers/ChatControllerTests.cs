namespace BankingApp.Web.Tests.Controllers;

using Application.Features.Chat.Services;
using BankingApp.Contracts.Features.Chat.Dtos;
using BankingApp.Web.Controllers;
using BankingApp.Web.Models.Chat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class ChatControllerTests : IDisposable
{
    private readonly Mock<IChatService> _chatService = new(MockBehavior.Strict);
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        DefaultHttpContext httpContext = new();
        _controller = new ChatController(_chatService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public async Task Index_WhenServiceReturnsSessions_ReturnsViewWithSessions()
    {
        List<ChatSessionDto> sessions =
        [
            new ChatSessionDto { Id = 1, Subject = "Billing query", Status = "Open" },
            new ChatSessionDto { Id = 2, Subject = "Card issue", Status = "Closed" },
        ];
        _chatService
            .Setup(service => service.GetSessionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        IActionResult result = await _controller.Index();

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        ChatIndexViewModel model = viewResult.Model.Should().BeOfType<ChatIndexViewModel>().Subject;
        model.Sessions.Should().HaveCount(2);
        model.ErrorMessage.Should().BeNull();
        _chatService.VerifyAll();
    }

    [Fact]
    public async Task Index_WhenServiceReturnsError_ReturnsEmptySessionsListWithErrorMessage()
    {
        _chatService
            .Setup(service => service.GetSessionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unexpected("Chat.Error", "Session service unavailable"));

        IActionResult result = await _controller.Index();

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        ChatIndexViewModel model = viewResult.Model.Should().BeOfType<ChatIndexViewModel>().Subject;
        model.Sessions.Should().BeEmpty();
        model.ErrorMessage.Should().Be("Session service unavailable");
        _chatService.VerifyAll();
    }

    [Fact]
    public async Task Details_WhenSessionNotFound_ReturnsNotFound()
    {
        _chatService
            .Setup(service => service.GetSessionAsync(It.IsAny<int>(), 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound("Chat.NotFound", "Session not found"));

        IActionResult result = await _controller.Details(99);

        result.Should().BeOfType<NotFoundResult>();
        _chatService.VerifyAll();
    }

    [Fact]
    public async Task Details_WhenSessionFound_ReturnsViewWithMessages()
    {
        ChatSessionDto session = new()
        {
            Id = 5,
            Subject = "Help needed",
            Status = "Open",
            Messages = [new ChatMessageDto { Id = 1, Content = "Hello", Sender = "Customer" }],
        };
        _chatService
            .Setup(service => service.GetSessionAsync(It.IsAny<int>(), 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        IActionResult result = await _controller.Details(5);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        ChatDetailsViewModel model = viewResult.Model.Should().BeOfType<ChatDetailsViewModel>().Subject;
        model.Session.Should().Be(session);
        model.Messages.Should().ContainSingle();
        _chatService.VerifyAll();
    }

    [Fact]
    public async Task Create_WhenServiceSucceeds_RedirectsToDetails()
    {
        ChatSessionDto newSession = new() { Id = 10, Subject = "General", Status = "Open" };
        _chatService
            .Setup(service => service.CreateSessionAsync(It.IsAny<int>(), "General", It.IsAny<CancellationToken>()))
            .ReturnsAsync(newSession);

        IActionResult result = await _controller.Create("General");

        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.RouteValues!["id"].Should().Be(10);
        _chatService.VerifyAll();
    }

    [Fact]
    public async Task Create_WhenServiceReturnsError_RedirectsToIndexWithTempDataError()
    {
        _chatService
            .Setup(service => service.CreateSessionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure("Chat.Failed", "Could not open session"));

        IActionResult result = await _controller.Create("Billing");

        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["Error"].Should().Be("Could not open session");
        _chatService.VerifyAll();
    }
}
