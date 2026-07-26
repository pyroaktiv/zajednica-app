using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.Community.Api.Dto.Memberships;
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

    [HttpGet("members/me")]
    public ActionResult<MemberProfileDto> GetMine(Guid communityId)
    {
        return Ok(_memberships.GetMine(User.AccountId(), communityId));
    }

    [HttpPut("members/me/unit-number")]
    public ActionResult<UnitNumberDto> SetUnitNumber(Guid communityId, [FromBody] SetUnitNumberRequest request)
    {
        return Ok(_memberships.SetUnitNumber(User.AccountId(), communityId, request));
    }

    [HttpGet("members")]
    public ActionResult<IReadOnlyList<MemberSummaryDto>> GetConfirmed(Guid communityId)
    {
        return Ok(_memberships.GetConfirmed(User.AccountId(), communityId));
    }

    [HttpGet("members/issuers")]
    public ActionResult<IReadOnlyList<MemberSummaryDto>> GetIssuers(Guid communityId)
    {
        return Ok(_memberships.GetIssuers(User.AccountId(), communityId));
    }

    [HttpGet("members/unconfirmed")]
    public ActionResult<IReadOnlyList<MemberSummaryDto>> GetUnconfirmed(Guid communityId)
    {
        return Ok(_memberships.GetUnconfirmed(User.AccountId(), communityId));
    }

    [HttpGet("members/manager")]
    public ActionResult<MemberSummaryDto> GetManager(Guid communityId)
    {
        var manager = _memberships.GetManager(User.AccountId(), communityId);
        return manager is null ? NoContent() : Ok(manager);
    }

    [HttpGet("members/{membershipId:guid}")]
    public ActionResult<MemberProfileDto> Get(Guid communityId, Guid membershipId)
    {
        return Ok(_memberships.Get(User.AccountId(), communityId, membershipId));
    }

    [HttpPost("members/{membershipId:guid}/roles/issuer")]
    public IActionResult GrantIssuer(Guid communityId, Guid membershipId)
    {
        _memberships.GrantIssuer(User.AccountId(), communityId, membershipId);
        return NoContent();
    }

    [HttpGet("ranking")]
    public ActionResult<IReadOnlyList<MemberSummaryDto>> GetRanking(Guid communityId)
    {
        return Ok(_memberships.GetRanking(User.AccountId(), communityId));
    }
}
