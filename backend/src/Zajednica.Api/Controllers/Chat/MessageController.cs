using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Messages;
using Zajednica.Chat.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Chat;

[ApiController]
[Authorize]
[Route("api/communities/{communityId:guid}/chats/{chatId:guid}/messages")]
public sealed class MessageController : ControllerBase
{
    private readonly IMessageService _messages;

    public MessageController(IMessageService messages)
    {
        _messages = messages;
    }

    [HttpPost]
    public ActionResult<MessageDto> SendText(Guid communityId, Guid chatId, [FromBody] SendTextRequest request)
    {
        return Ok(_messages.SendText(User.AccountId(), communityId, chatId, request));
    }

    [HttpPost("voice")]
    public ActionResult<MessageDto> SendVoice(Guid communityId, Guid chatId, [FromBody] SendVoiceRequest request)
    {
        return Ok(_messages.SendVoice(User.AccountId(), communityId, chatId, request));
    }

    [HttpPost("read")]
    public IActionResult MarkRead(Guid communityId, Guid chatId)
    {
        _messages.MarkRead(User.AccountId(), communityId, chatId);
        return NoContent();
    }

    [HttpGet]
    public ActionResult<CursorPage<MessageDto, PageCursor>> GetPage(Guid communityId, Guid chatId, [FromQuery] PageCursor? after, [FromQuery] int limit)
    {
        return Ok(_messages.GetPage(User.AccountId(), communityId, chatId, after, limit));
    }
}
