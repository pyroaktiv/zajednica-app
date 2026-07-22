using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Identity;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController : ControllerBase
{
    private readonly IProfileService _profiles;

    public ProfileController(IProfileService profiles)
    {
        _profiles = profiles;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetMine(CancellationToken ct)
    {
        return Ok(await _profiles.GetAsync(User.AccountId(), ct));
    }

    [HttpGet("{accountId:guid}")]
    public async Task<ActionResult<ProfileDto>> Get(Guid accountId, CancellationToken ct)
    {
        return Ok(await _profiles.GetAsync(accountId, ct));
    }

    [HttpPut]
    public async Task<ActionResult<ProfileDto>> Update([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        return Ok(await _profiles.UpdateAsync(User.AccountId(), request, ct));
    }
}
