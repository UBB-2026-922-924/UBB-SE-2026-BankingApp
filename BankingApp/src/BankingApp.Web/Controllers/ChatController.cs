namespace BankingApp.Web.Controllers;

using Application.Features.Chat.Services;
using Contracts.Features.Chat.Dtos;
using Contracts.Http;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Models.Chat;

public class ChatController(IChatService chatService) : Controller
{
    public async Task<IActionResult> Index()
    {
        ErrorOr<List<ChatSessionDto>> sessions = await chatService.GetSessionsAsync(CurrentUserId);
        var viewModel = new ChatIndexViewModel
        {
            Sessions = sessions.IsError ? [] : sessions.Value,
            ErrorMessage = sessions.IsError ? sessions.FirstError.Description : null,
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string issueCategory)
    {
        ErrorOr<ChatSessionDto> response = await chatService.CreateSessionAsync(CurrentUserId, issueCategory);
        if (response.IsError)
        {
            TempData["Error"] = response.FirstError.Description;
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Details), new { id = response.Value.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        ErrorOr<ChatSessionDto> session = await chatService.GetSessionAsync(CurrentUserId, id);
        if (session.IsError)
        {
            return NotFound();
        }

        var viewModel = new ChatDetailsViewModel
        {
            Session = session.Value,
            Messages = session.Value.Messages,
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(int sessionId, string content)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            await chatService.PostMessageAsync(CurrentUserId, sessionId, content);
        }

        return RedirectToAction(nameof(Details), new { id = sessionId });
    }

    [HttpPost]
    public async Task<IActionResult> EndSession(int id)
    {
        await chatService.CloseSessionAsync(CurrentUserId, id);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SubmitFeedback(int id, int rating, string? feedback)
    {
        await chatService.SaveFeedbackAsync(CurrentUserId, id, rating, feedback);
        return RedirectToAction(nameof(Index));
    }

    private int CurrentUserId => int.TryParse(User.FindFirst(AuthClaimTypes.UserId)?.Value, out int userId) ? userId : 0;
}
