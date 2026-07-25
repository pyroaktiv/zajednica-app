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
    private readonly IIntentService _intents;

    public IntentController(IIntentService intents)
    {
        _intents = intents;
    }

    [HttpPost("ban")]
    public ActionResult<UserTargetingIntentDetailsDto> OpenBan(Guid communityId, [FromBody] OpenUserTargetingIntentRequest request)
    {
        var opened = _intents.OpenBan(User.AccountId(), communityId, request);
        return CreatedAtAction(nameof(Get), new { communityId, intentId = opened.Id }, opened);
    }

    [HttpPost("manager-election")]
    public ActionResult<UserTargetingIntentDetailsDto> OpenManagerElection(Guid communityId, [FromBody] OpenUserTargetingIntentRequest request)
    {
        var opened = _intents.OpenManagerElection(User.AccountId(), communityId, request);
        return CreatedAtAction(nameof(Get), new { communityId, intentId = opened.Id }, opened);
    }

    [HttpPost("{intentId:guid}/votes")]
    public ActionResult<UserTargetingIntentDetailsDto> Vote(Guid communityId, Guid intentId, [FromBody] CastVoteRequest request)
    {
        return Ok(_intents.Vote(User.AccountId(), communityId, intentId, request));
    }

    [HttpGet("{intentId:guid}")]
    public ActionResult<UserTargetingIntentDetailsDto> Get(Guid communityId, Guid intentId)
    {
        return Ok(_intents.Get(User.AccountId(), communityId, intentId));
    }

    [HttpGet]
    public ActionResult<Page<UserTargetingIntentSummaryDto>> GetPage(Guid communityId, [FromQuery] DateTime? before, [FromQuery] int limit)
    {
        return Ok(_intents.GetPage(User.AccountId(), communityId, before, limit));
    }
}
