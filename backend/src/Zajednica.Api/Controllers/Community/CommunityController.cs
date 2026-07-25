using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Dto.Memberships;
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
    public ActionResult<CommunityDetailsDto> Create([FromBody] CreateCommunityRequest request)
    {
        var created = _communities.Create(User.AccountId(), request);
        return CreatedAtAction(nameof(Get), new { communityId = created.Id }, created);
    }

    [HttpGet("mine")]
    public ActionResult<IReadOnlyList<MyCommunityDto>> GetMine()
    {
        return Ok(_communities.GetMine(User.AccountId()));
    }

    [HttpGet("{communityId:guid}")]
    public ActionResult<CommunityDetailsDto> Get(Guid communityId)
    {
        return Ok(_communities.Get(User.AccountId(), communityId));
    }

    [HttpPut("{communityId:guid}")]
    public ActionResult<CommunityDetailsDto> Update(Guid communityId, [FromBody] UpdateCommunityRequest request)
    {
        return Ok(_communities.Update(User.AccountId(), communityId, request));
    }

    [HttpGet("{communityId:guid}/qr")]
    public ActionResult<CommunityQrDto> GetQr(Guid communityId)
    {
        return Ok(_communities.GetQr(User.AccountId(), communityId));
    }

    [HttpPost("join")]
    public ActionResult<MemberProfileDto> Join([FromBody] JoinCommunityRequest request)
    {
        return Ok(_communities.Join(User.AccountId(), request));
    }

    [HttpPost("{communityId:guid}/leave")]
    public IActionResult Leave(Guid communityId)
    {
        _communities.Leave(User.AccountId(), communityId);
        return NoContent();
    }
}
