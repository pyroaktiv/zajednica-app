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

    /// <summary>Creates the account and sends the activation email. The account cannot log in until verified.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAccountRequest request, CancellationToken ct)
    {
        await _auth.RegisterAsync(request, ct);
        return Ok();
    }

    /// <summary>Activates the account from the token in the emailed link.</summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct)
    {
        await _auth.VerifyEmailAsync(request, ct);
        return Ok();
    }

    /// <summary>Username or email + password → a fresh access/refresh token pair.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokens>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        return Ok(await _auth.LoginAsync(request, ct));
    }

    /// <summary>Rotates a valid refresh token for a new token pair (the presented token is single-use).</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokens>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        return Ok(await _auth.RefreshAsync(request, ct));
    }

    /// <summary>Revokes the presented refresh token, ending that session. Idempotent.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await _auth.LogoutAsync(request, ct);
        return Ok();
    }
}
