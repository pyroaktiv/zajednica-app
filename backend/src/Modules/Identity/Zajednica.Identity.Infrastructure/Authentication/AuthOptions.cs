using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Infrastructure.Authentication;


public sealed class AuthOptions : IAuthTokenSettings
{
    public const string SectionName = "Auth";

    public int RefreshTokenDays { get; set; } = 30;
    public int EmailVerificationTokenHours { get; set; } = 24;
    public string EmailActivationUrl { get; set; } = "";
}
