using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Dto.HelpChats;
using Zajednica.Chat.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Chat;

[ApiController]
[Authorize]
[Route("api/communities/{communityId:guid}/chats")]
public sealed class HelpRequestChatController : ControllerBase
{
    private readonly IHelpRequestChatService _helpChats;

    public HelpRequestChatController(IHelpRequestChatService helpChats)
    {
        _helpChats = helpChats;
    }

    [HttpPost("help-requests/{helpRequestId:guid}")]
    public ActionResult<ChatDetailsDto> Respond(Guid communityId, Guid helpRequestId)
    {
        return Ok(_helpChats.Respond(User.AccountId(), communityId, helpRequestId));
    }

    [HttpPost("{chatId:guid}/conclude-with-reward")]
    public ActionResult<ChatDetailsDto> ConcludeWithReward(Guid communityId, Guid chatId, [FromBody] ConcludeWithRewardRequestDto requestDto)
    {
        return Ok(_helpChats.ConcludeWithReward(User.AccountId(), communityId, chatId, requestDto));
    }

    [HttpPost("{chatId:guid}/conclude-without-reward")]
    public ActionResult<ChatDetailsDto> ConcludeWithoutReward(Guid communityId, Guid chatId)
    {
        return Ok(_helpChats.ConcludeWithoutReward(User.AccountId(), communityId, chatId));
    }

    [HttpPost("{chatId:guid}/resign")]
    public ActionResult<ChatDetailsDto> Resign(Guid communityId, Guid chatId)
    {
        return Ok(_helpChats.Resign(User.AccountId(), communityId, chatId));
    }
}
