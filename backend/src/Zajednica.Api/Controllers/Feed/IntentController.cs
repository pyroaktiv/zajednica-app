using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Feed;

[ApiController]
[Authorize]
[Route("api/communities/{communityId:guid}/intents")]
public sealed class IntentController : ControllerBase
{
    private readonly IIntentCommandService _intentsCommand;
    private readonly IIntentQueryService _queries;

    public IntentController(IIntentCommandService intentsCommand, IIntentQueryService queries)
    {
        _intentsCommand = intentsCommand;
        _queries = queries;
    }

    [HttpPost("ban")]
    public ActionResult<IntentDetailsDto> OpenBan(Guid communityId, [FromBody] OpenUserTargetingIntentRequestDto requestDto)
    {
        var opened = _intentsCommand.OpenBan(User.AccountId(), communityId, requestDto);
        return CreatedAtAction(nameof(Get), new { communityId, intentId = opened.Id }, opened);
    }

    [HttpPost("manager-election")]
    public ActionResult<IntentDetailsDto> OpenManagerElection(Guid communityId, [FromBody] OpenUserTargetingIntentRequestDto requestDto)
    {
        var opened = _intentsCommand.OpenManagerElection(User.AccountId(), communityId, requestDto);
        return CreatedAtAction(nameof(Get), new { communityId, intentId = opened.Id }, opened);
    }

    [HttpPost("mute")]
    public ActionResult<IntentDetailsDto> OpenMute(Guid communityId, [FromBody] OpenUserTargetingIntentRequestDto requestDto)
    {
        var opened = _intentsCommand.OpenMute(User.AccountId(), communityId, requestDto);
        return CreatedAtAction(nameof(Get), new { communityId, intentId = opened.Id }, opened);
    }

    [HttpPost("{intentId:guid}/votes")]
    public ActionResult<IntentDetailsDto> Vote(Guid communityId, Guid intentId, [FromBody] CastVoteRequestDto requestDto)
    {
        return Ok(_intentsCommand.Vote(User.AccountId(), communityId, intentId, requestDto));
    }

    [HttpGet("{intentId:guid}")]
    public ActionResult<IntentDetailsDto> Get(Guid communityId, Guid intentId)
    {
        return Ok(_queries.Get(User.AccountId(), communityId, intentId));
    }

    [HttpGet("{intentId:guid}/votes")]
    public ActionResult<IReadOnlyList<IntentVoterDto>> GetVotes(Guid communityId, Guid intentId)
    {
        return Ok(_queries.GetVotes(User.AccountId(), communityId, intentId));
    }

    [HttpGet]
    public ActionResult<CursorPage<IntentSummaryDto, PageCursor>> GetPage(Guid communityId, [FromQuery] PageCursor? before, [FromQuery] int limit)
    {
        return Ok(_queries.GetPage(User.AccountId(), communityId, before, limit));
    }
}
