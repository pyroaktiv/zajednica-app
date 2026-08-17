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
    public ActionResult<ProfileDto> GetMine()
    {
        return Ok(_profiles.Get(User.AccountId()));
    }

    [HttpPut]
    public ActionResult<ProfileDto> Update([FromBody] UpdateProfileRequestDto requestDto)
    {
        return Ok(_profiles.Update(User.AccountId(), requestDto));
    }

    [HttpPut("image")]
    public ActionResult<ProfileDto> SetImage([FromBody] SetProfileImageRequestDto requestDto)
    {
        return Ok(_profiles.SetImage(User.AccountId(), requestDto));
    }

    [HttpDelete("image")]
    public IActionResult RemoveImage()
    {
        _profiles.RemoveImage(User.AccountId());
        return NoContent();
    }
}
