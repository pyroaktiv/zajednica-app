using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Api.Public;

public interface IAuthenticationService
{
    Task RegisterAsync(RegisterAccountRequest request, CancellationToken ct = default);
    Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
    Task<AuthTokens> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthTokens> RefreshAsync(RefreshRequest request, CancellationToken ct = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken ct = default);
}
