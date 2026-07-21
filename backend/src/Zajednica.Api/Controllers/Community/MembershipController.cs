using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.Community.Api.Dto;
using Zajednica.Community.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Community;

[ApiController]
[Authorize]
[Route("api/communities/{communityId:guid}")]
public sealed class MembershipController : ControllerBase
{
    private readonly IMembershipService _memberships;

    public MembershipController(IMembershipService memberships)
    {
        _memberships = memberships;
    }

    [HttpGet("me")]
    public async Task<ActionResult<MembershipDto>> GetMine(Guid communityId, CancellationToken ct)
    {
        return Ok(await _memberships.GetMineAsync(User.AccountId(), communityId, ct));
    }

    [HttpPut("me/unit-number")]
    public async Task<ActionResult<MembershipDto>> SetUnitNumber(Guid communityId, [FromBody] SetUnitNumberRequest request, CancellationToken ct)
    {
        return Ok(await _memberships.SetUnitNumberAsync(User.AccountId(), communityId, request, ct));
    }

    [HttpGet("members")]
    public async Task<ActionResult<IReadOnlyList<CommunityMemberDto>>> GetConfirmed(Guid communityId, CancellationToken ct)
    {
        return Ok(await _memberships.GetConfirmedAsync(User.AccountId(), communityId, ct));
    }

    [HttpGet("members/issuers")]
    public async Task<ActionResult<IReadOnlyList<CommunityMemberDto>>> GetIssuers(Guid communityId, CancellationToken ct)
    {
        return Ok(await _memberships.GetIssuersAsync(User.AccountId(), communityId, ct));
    }

    [HttpGet("members/unconfirmed")]
    public async Task<ActionResult<IReadOnlyList<CommunityMemberDto>>> GetUnconfirmed(Guid communityId, CancellationToken ct)
    {
        return Ok(await _memberships.GetUnconfirmedAsync(User.AccountId(), communityId, ct));
    }

    [HttpGet("members/manager")]
    public async Task<ActionResult<CommunityMemberDto>> GetManager(Guid communityId, CancellationToken ct)
    {
        var manager = await _memberships.GetManagerAsync(User.AccountId(), communityId, ct);
        return manager is null ? NoContent() : Ok(manager);
    }

    [HttpGet("members/{membershipId:guid}")]
    public async Task<ActionResult<MembershipDto>> Get(Guid communityId, Guid membershipId, CancellationToken ct)
    {
        return Ok(await _memberships.GetAsync(User.AccountId(), communityId, membershipId, ct));
    }

    [HttpPost("members/{membershipId:guid}/roles/issuer")]
    public async Task<IActionResult> GrantIssuer(Guid communityId, Guid membershipId, CancellationToken ct)
    {
        await _memberships.GrantIssuerAsync(User.AccountId(), communityId, membershipId, ct);
        return NoContent();
    }

    [HttpGet("ranking")]
    public async Task<ActionResult<IReadOnlyList<CommunityMemberDto>>> GetRanking(Guid communityId, CancellationToken ct)
    {
        return Ok(await _memberships.GetRankingAsync(User.AccountId(), communityId, ct));
    }
}
