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
    public async Task<ActionResult<IntentDetailsDto>> OpenBan(Guid communityId, [FromBody] OpenIntentRequest request, CancellationToken ct)
    {
        var opened = await _intents.OpenBanAsync(User.AccountId(), communityId, request, ct);
        return CreatedAtAction(nameof(Get), new { communityId, intentId = opened.Id }, opened);
    }

    [HttpPost("manager-election")]
    public async Task<ActionResult<IntentDetailsDto>> OpenManagerElection(Guid communityId, [FromBody] OpenIntentRequest request, CancellationToken ct)
    {
        var opened = await _intents.OpenManagerElectionAsync(User.AccountId(), communityId, request, ct);
        return CreatedAtAction(nameof(Get), new { communityId, intentId = opened.Id }, opened);
    }

    [HttpPost("{intentId:guid}/votes")]
    public async Task<ActionResult<IntentDetailsDto>> Vote(Guid communityId, Guid intentId, [FromBody] CastVoteRequest request, CancellationToken ct)
    {
        return Ok(await _intents.VoteAsync(User.AccountId(), communityId, intentId, request, ct));
    }

    [HttpGet("{intentId:guid}")]
    public async Task<ActionResult<IntentDetailsDto>> Get(Guid communityId, Guid intentId, CancellationToken ct)
    {
        return Ok(await _intents.GetAsync(User.AccountId(), communityId, intentId, ct));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<IntentSummaryDto>>> GetPaged(Guid communityId, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
    {
        return Ok(await _intents.GetPagedAsync(User.AccountId(), communityId, page, pageSize, ct));
    }
}
