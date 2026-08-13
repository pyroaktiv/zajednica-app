using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.Storage;
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
    private readonly IFileStorage _storage;

    public MessageController(IMessageService messages, IFileStorage storage)
    {
        _messages = messages;
        _storage = storage;
    }

    [HttpPost]
    public ActionResult<MessageDto> SendText(Guid communityId, Guid chatId, [FromBody] SendTextRequestDto requestDto)
    {
        return Ok(_messages.SendText(User.AccountId(), communityId, chatId, requestDto));
    }

    [HttpPost("voice")]
    public ActionResult<MessageDto> SendVoice(Guid communityId, Guid chatId, [FromBody] SendVoiceRequestDto requestDto)
    {
        return Ok(_messages.SendVoice(User.AccountId(), communityId, chatId, requestDto));
    }

    [HttpPost("read")]
    public IActionResult MarkRead(Guid communityId, Guid chatId)
    {
        _messages.MarkRead(User.AccountId(), communityId, chatId);
        return NoContent();
    }

    [HttpGet]
    public ActionResult<CursorPage<MessageDto, PageCursor>> GetPage(Guid communityId, Guid chatId, [FromQuery] PageCursor? before, [FromQuery] int limit)
    {
        return Ok(_messages.GetPage(User.AccountId(), communityId, chatId, before, limit));
    }

    [HttpGet("{messageId:guid}/audio")]
    public IActionResult Audio(Guid communityId, Guid chatId, Guid messageId)
    {
        var reference = _messages.GetAudioContent(User.AccountId(), communityId, chatId, messageId);
        var file = _storage.Open(reference.Key);
        if (file is null)
            return NotFound();

        Response.Headers.CacheControl = "private, max-age=300";
        return File(file.Content, file.ContentType, reference.DownloadName, enableRangeProcessing: true);
    }
}
