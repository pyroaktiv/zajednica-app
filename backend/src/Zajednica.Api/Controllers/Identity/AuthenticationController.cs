using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;

namespace Zajednica.Api.Controllers.Identity;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _auth;

    public AuthenticationController(IAuthenticationService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterAccountRequestDto requestDto)
    {
        _auth.Register(requestDto);
        return Ok();
    }

    [HttpPost("verify-email")]
    public IActionResult VerifyEmail([FromBody] VerifyEmailRequestDto requestDto)
    {
        _auth.VerifyEmail(requestDto);
        return Ok();
    }

    [HttpPost("login")]
    public ActionResult<AuthTokensDto> Login([FromBody] LoginRequestDto requestDto)
    {
        return Ok(_auth.Login(requestDto));
    }

    [HttpPost("refresh")]
    public ActionResult<AuthTokensDto> Refresh([FromBody] RefreshRequestDto requestDto)
    {
        return Ok(_auth.Refresh(requestDto));
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LogoutRequestDto requestDto)
    {
        _auth.Logout(requestDto);
        return Ok();
    }
}
