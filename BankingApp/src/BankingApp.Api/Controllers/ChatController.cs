namespace BankingApp.Api.Controllers;

using Application.Features.Chat.Services;
using Contracts.Features.Chat.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Manages the authenticated user's chat support sessions.
/// </summary>
[ApiController]
[Authorize]
[Route(ApiEndpoints.Chat.Base)]
public class ChatController(IChatService chatService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await chatService.GetSessionsAsync(userId, cancellationToken), Ok);
    }

    [HttpGet(ApiEndpoints.Chat.ById)]
    public async Task<IActionResult> GetSession(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await chatService.GetSessionAsync(userId, id, cancellationToken), Ok);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreateChatSessionRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await chatService.CreateSessionAsync(userId, request.Subject, cancellationToken),
            session => CreatedAtAction(nameof(GetSession), new { id = session.Id }, session));
    }

    [HttpPost(ApiEndpoints.Chat.Messages)]
    public async Task<IActionResult> PostMessage(
        int id,
        [FromBody] CreateChatMessageRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await chatService.PostMessageAsync(userId, id, request.Content, cancellationToken),
            Ok);
    }

    [HttpPut(ApiEndpoints.Chat.ById)]
    public async Task<IActionResult> CloseSession(int id, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await chatService.CloseSessionAsync(userId, id, cancellationToken));
    }

    [HttpPost(ApiEndpoints.Chat.Feedback)]
    public async Task<IActionResult> SaveFeedback(
        int id,
        [FromBody] SaveChatFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await chatService.SaveFeedbackAsync(userId, id, request.Rating, request.Feedback, cancellationToken));
    }
}