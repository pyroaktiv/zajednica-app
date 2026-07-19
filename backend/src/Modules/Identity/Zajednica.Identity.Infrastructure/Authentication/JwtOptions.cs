namespace Zajednica.Identity.Infrastructure.Authentication;

/// <summary>
/// Bound from the "Jwt" configuration section — the same section the host's AuthConfiguration reads
/// for validation, so signing and validation agree. Externalized via env vars
/// (Jwt__Key, Jwt__Issuer, ...); appsettings holds dev defaults.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
}
