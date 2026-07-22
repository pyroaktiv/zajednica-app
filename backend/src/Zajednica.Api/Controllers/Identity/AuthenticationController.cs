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
    public async Task<IActionResult> Register([FromBody] RegisterAccountRequest request, CancellationToken ct)
    {
        await _auth.RegisterAsync(request, ct);
        return Ok();
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct)
    {
        await _auth.VerifyEmailAsync(request, ct);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthTokens>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        return Ok(await _auth.LoginAsync(request, ct));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokens>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        return Ok(await _auth.RefreshAsync(request, ct));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await _auth.LogoutAsync(request, ct);
        return Ok();
    }
}
