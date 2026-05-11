using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;

namespace NEmaris.API.Controllers;

[ApiController]
[Route("api/chat")]
[AllowAnonymous]
[EnableRateLimiting("chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _chatService.ChatAsync(request, cancellationToken);
        return Ok(response);
    }
}
