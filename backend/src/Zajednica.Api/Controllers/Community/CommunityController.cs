using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.Community.Api.Dto;
using Zajednica.Community.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Community;

[ApiController]
[Authorize]
[Route("api/communities")]
public sealed class CommunityController : ControllerBase
{
    private readonly ICommunityService _communities;

    public CommunityController(ICommunityService communities)
    {
        _communities = communities;
    }

    [HttpPost]
    public async Task<ActionResult<CommunityDto>> Create([FromBody] CreateCommunityRequest request, CancellationToken ct)
    {
        var created = await _communities.CreateAsync(User.AccountId(), request, ct);
        return CreatedAtAction(nameof(Get), new { communityId = created.Id }, created);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<CommunitySummaryDto>>> GetMine(CancellationToken ct)
    {
        return Ok(await _communities.GetMineAsync(User.AccountId(), ct));
    }

    [HttpGet("{communityId:guid}")]
    public async Task<ActionResult<CommunityDto>> Get(Guid communityId, CancellationToken ct)
    {
        return Ok(await _communities.GetAsync(User.AccountId(), communityId, ct));
    }

    [HttpPut("{communityId:guid}")]
    public async Task<ActionResult<CommunityDto>> Update(Guid communityId, [FromBody] UpdateCommunityRequest request, CancellationToken ct)
    {
        return Ok(await _communities.UpdateAsync(User.AccountId(), communityId, request, ct));
    }

    [HttpGet("{communityId:guid}/qr")]
    public async Task<ActionResult<CommunityQrDto>> GetQr(Guid communityId, CancellationToken ct)
    {
        return Ok(await _communities.GetQrAsync(User.AccountId(), communityId, ct));
    }

    [HttpPost("join")]
    public async Task<ActionResult<MembershipDto>> Join([FromBody] JoinCommunityRequest request, CancellationToken ct)
    {
        return Ok(await _communities.JoinAsync(User.AccountId(), request, ct));
    }

    [HttpPost("{communityId:guid}/leave")]
    public async Task<IActionResult> Leave(Guid communityId, CancellationToken ct)
    {
        await _communities.LeaveAsync(User.AccountId(), communityId, ct);
        return NoContent();
    }
}
