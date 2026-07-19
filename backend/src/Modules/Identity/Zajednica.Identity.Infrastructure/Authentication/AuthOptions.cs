using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Infrastructure.Authentication;

/// <summary>
/// Bound from the "Auth" configuration section, alongside <see cref="JwtOptions"/> and SmtpOptions.
/// The Infrastructure adapter for the Core <see cref="IAuthTokenSettings"/> port: it carries the
/// token-lifecycle knobs (refresh / activation lifetimes, activation URL) and is registered through
/// the standard options pattern. Externalized via env vars (Auth__RefreshTokenDays, ...).
/// </summary>
public sealed class AuthOptions : IAuthTokenSettings
{
    public const string SectionName = "Auth";

    public int RefreshTokenDays { get; set; } = 30;
    public int EmailVerificationTokenHours { get; set; } = 24;
    public string EmailActivationUrl { get; set; } = "";
}
