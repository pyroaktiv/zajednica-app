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
    public IActionResult Register([FromBody] RegisterAccountRequest request)
    {
        _auth.Register(request);
        return Ok();
    }

    [HttpPost("verify-email")]
    public IActionResult VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        _auth.VerifyEmail(request);
        return Ok();
    }

    [HttpPost("login")]
    public ActionResult<AuthTokens> Login([FromBody] LoginRequest request)
    {
        return Ok(_auth.Login(request));
    }

    [HttpPost("refresh")]
    public ActionResult<AuthTokens> Refresh([FromBody] RefreshRequest request)
    {
        return Ok(_auth.Refresh(request));
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LogoutRequest request)
    {
        _auth.Logout(request);
        return Ok();
    }
}
