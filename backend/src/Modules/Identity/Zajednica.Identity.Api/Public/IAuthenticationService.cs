using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Api.Public;

public interface IAuthenticationService
{
    void Register(RegisterAccountRequest request);
    void VerifyEmail(VerifyEmailRequest request);
    AuthTokens Login(LoginRequest request);
    AuthTokens Refresh(RefreshRequest request);
    void Logout(LogoutRequest request);
}
