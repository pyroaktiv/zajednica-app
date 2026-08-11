using Microsoft.Extensions.Configuration;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Infrastructure.Authentication;

public sealed class AuthTokenSettings(IConfiguration configuration) : IAuthTokenSettings
{
    public int RefreshTokenDays { get; } = configuration.GetValue("Auth:RefreshTokenDays", 30);

    public int EmailVerificationTokenHours { get; } = configuration.GetValue("Auth:EmailVerificationTokenHours", 24);
}
