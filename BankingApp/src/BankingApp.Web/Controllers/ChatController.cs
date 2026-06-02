namespace BankingApp.Web.Controllers;

using Infrastructure.Http.Features.Chat.Services;
using Contracts.Features.Chat.Dtos;
using Microsoft.AspNetCore.Mvc;
using Models.Chat;

public class ChatController(IChatRepoProxy chatRepoProxy) : Controller
{
    public async Task<IActionResult> Index()
    {
        try
        {
            List<ChatSessionDto> sessions = await chatRepoProxy.GetSessionsAsync();
            var viewModel = new ChatIndexViewModel { Sessions = sessions };
            return View(viewModel);
        }
        catch (Exception exception)
        {
            var viewModel = new ChatIndexViewModel
            {
                Sessions = [],
                ErrorMessage = exception.Message,
            };
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string issueCategory)
    {
        try
        {
            ChatSessionDto response = await chatRepoProxy.CreateSessionAsync(issueCategory);
            return RedirectToAction(nameof(Details), new { id = response.Id });
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            ChatSessionDto session = await chatRepoProxy.GetSessionAsync(id);
            var viewModel = new ChatDetailsViewModel
            {
                Session = session,
                Messages = session.Messages,
            };
            return View(viewModel);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int sessionId, string content)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                await chatRepoProxy.CreateMessageAsync(sessionId, content);
            }
            catch (Exception exception)
            {
                TempData["Error"] = exception.Message;
            }
        }

        return RedirectToAction(nameof(Details), new { id = sessionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EndSession(int id)
    {
        try
        {
            await chatRepoProxy.CloseSessionAsync(id);
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitFeedback(int id, int rating, string? feedback)
    {
        try
        {
            await chatRepoProxy.SaveFeedbackAsync(id, rating, feedback);
            TempData["Success"] = "Thank you for your feedback.";
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
