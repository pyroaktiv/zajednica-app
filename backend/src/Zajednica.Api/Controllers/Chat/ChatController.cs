using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Chat;

[ApiController]
[Authorize]
[Route("api/communities/{communityId:guid}/chats")]
public sealed class ChatController : ControllerBase
{
    private readonly IChatService _chats;

    public ChatController(IChatService chats)
    {
        _chats = chats;
    }

    [HttpPost("direct")]
    public ActionResult<ChatDetailsDto> OpenDirect(Guid communityId, [FromBody] OpenDirectChatRequest request)
    {
        var opened = _chats.OpenDirect(User.AccountId(), communityId, request);
        return CreatedAtAction(nameof(Get), new { communityId, chatId = opened.Id }, opened);
    }

    [HttpPost("temporary")]
    public ActionResult<ChatDetailsDto> OpenTemporary(Guid communityId, [FromBody] OpenTemporaryChatRequest request)
    {
        var opened = _chats.OpenTemporary(User.AccountId(), communityId, request);
        return CreatedAtAction(nameof(Get), new { communityId, chatId = opened.Id }, opened);
    }

    [HttpGet("{chatId:guid}")]
    public ActionResult<ChatDetailsDto> Get(Guid communityId, Guid chatId)
    {
        return Ok(_chats.Get(User.AccountId(), communityId, chatId));
    }

    [HttpGet("direct")]
    public ActionResult<CursorPage<ChatSummaryDto>> GetDirectPage(Guid communityId, [FromQuery] DateTime? before, [FromQuery] int limit)
    {
        return Ok(_chats.GetDirectPage(User.AccountId(), communityId, before, limit));
    }

    [HttpGet("help-requests")]
    public ActionResult<CursorPage<ChatSummaryDto>> GetHelpRequestPage(Guid communityId, [FromQuery] DateTime? before, [FromQuery] int limit)
    {
        return Ok(_chats.GetHelpRequestPage(User.AccountId(), communityId, before, limit));
    }

    [HttpGet("temporary")]
    public ActionResult<CursorPage<ChatSummaryDto>> GetTemporaryPage(Guid communityId, [FromQuery] DateTime? before, [FromQuery] int limit)
    {
        return Ok(_chats.GetTemporaryPage(User.AccountId(), communityId, before, limit));
    }
}
