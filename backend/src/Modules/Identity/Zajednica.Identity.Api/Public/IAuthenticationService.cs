using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Api.Public;

/// <summary>
/// Public application service of the Identity module — called by the module's own controller.
/// Implemented in Core/UseCases. Auth flow: Register creates the account and sends the activation
/// email; VerifyEmail activates it; Login (username or email + password) issues a token pair;
/// Refresh rotates it; Logout revokes the refresh token. JWT signing and the refresh-token store
/// live in Infrastructure — this contract stays transport-agnostic.
/// </summary>
public interface IAuthenticationService
{
    Task RegisterAsync(RegisterAccountRequest request, CancellationToken ct = default);
    Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
    Task<AuthTokens> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthTokens> RefreshAsync(RefreshRequest request, CancellationToken ct = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken ct = default);
}
