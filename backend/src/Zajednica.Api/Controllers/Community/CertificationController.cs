using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.Community.Api.Dto.Certification;
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
    public ActionResult<CertificationChallengeDto> CreateChallenge(Guid communityId)
    {
        return Ok(_certification.CreateChallenge(User.AccountId(), communityId));
    }

    [HttpDelete("{communityId:guid}/certification-challenges/{challengeId:guid}")]
    public IActionResult CancelChallenge(Guid communityId, Guid challengeId)
    {
        _certification.CancelChallenge(User.AccountId(), communityId, challengeId);
        return NoContent();
    }

    [HttpPost("certification-challenges/confirm")]
    public ActionResult<CertificationResultDto> Confirm([FromBody] ConfirmCertificationRequestDto requestDto)
    {
        return Ok(_certification.Confirm(User.AccountId(), requestDto));
    }
}
