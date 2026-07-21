using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.Community.Api.Dto;
using Zajednica.Community.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Community;

[ApiController]
[Authorize]
[Route("api/communities")]
public sealed class CertificationController : ControllerBase
{
    private readonly ICertificationService _certification;

    public CertificationController(ICertificationService certification)
    {
        _certification = certification;
    }

    [HttpPost("{communityId:guid}/certification-challenges")]
    public async Task<ActionResult<CertificationChallengeDto>> CreateChallenge(Guid communityId, CancellationToken ct)
    {
        return Ok(await _certification.CreateChallengeAsync(User.AccountId(), communityId, ct));
    }

    [HttpDelete("{communityId:guid}/certification-challenges/{challengeId:guid}")]
    public async Task<IActionResult> CancelChallenge(Guid communityId, Guid challengeId, CancellationToken ct)
    {
        await _certification.CancelChallengeAsync(User.AccountId(), communityId, challengeId, ct);
        return NoContent();
    }

    [HttpPost("certification-challenges/confirm")]
    public async Task<ActionResult<MembershipDto>> Confirm([FromBody] ConfirmCertificationRequest request, CancellationToken ct)
    {
        return Ok(await _certification.ConfirmAsync(User.AccountId(), request, ct));
    }

    [HttpGet("{communityId:guid}/trust-graph")]
    public async Task<ActionResult<TrustGraphDto>> GetTrustGraph(Guid communityId, CancellationToken ct)
    {
        return Ok(await _certification.GetTrustGraphAsync(User.AccountId(), communityId, ct));
    }
}
