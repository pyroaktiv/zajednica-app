using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Api.Public;

public interface IAuthenticationService
{
    void Register(RegisterAccountRequestDto requestDto);
    void VerifyEmail(VerifyEmailRequestDto requestDto);
    AuthTokensDto Login(LoginRequestDto requestDto);
    AuthTokensDto Refresh(RefreshRequestDto requestDto);
    void Logout(LogoutRequestDto requestDto);
}
