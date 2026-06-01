namespace BankingApp.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Features.Chat.Services;
using BankingApp.Contracts.Features.Chat.Dtos;
using ErrorOr;
using BankingApp.Web.Models.Chat;

public class ChatController : Controller
{

    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<IActionResult> Index()
    {
        ErrorOr<List<ChatSessionDto>> sessions = await _chatService.GetSessionsAsync();
        var viewModel = new ChatIndexViewModel
        {
            Sessions = sessions ?? new()
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string issueCategory)
    {
        ErrorOr<ChatSessionDto> response = await _chatService.CreateSessionAsync(issueCategory);
        if (response != null && response.Success)
        {
            return RedirectToAction("Details", new { id = response.SessionId });
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(int id)
    {
        ErrorOr<ChatSessionDto> session = await _chatService.GetSessionAsync(id);

        if (session == null)
        {
            return NotFound();
        }

        var messages = await _chatService.GetMessagesAsync(id);
        var viewModel = new ChatDetailsViewModel
        {
            Session = session,
            Messages = messages ?? new()
        };

        return View(viewModel);
    }

    [HttpPost]

    public async Task<IActionResult> SendMessage(int sessionId, string content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            await _chatService.CreateMessageAsync(sessionId, "User", content);
        }
        return RedirectToAction("Details", new { id = sessionId });
    }

    [HttpPost]

    public async Task<IActionResult> EndSession(int id)
    {
        await _chatService.UpdateSessionStatusAsync(id, "Closed");
        return Ok();
    }

    [HttpPost]

    public async Task<IActionResult> SubmitFeedback(int id, int rating, string feedback)
    {
        await _chatService.SaveFeedbackAsync(id, rating, feedback ?? string.Empty);
        return RedirectToAction("Index");
    }
}
