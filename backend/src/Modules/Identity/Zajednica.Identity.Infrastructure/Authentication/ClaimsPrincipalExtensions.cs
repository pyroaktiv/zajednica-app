using System.Security.Claims;

namespace Zajednica.Identity.Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid AccountId(this ClaimsPrincipal user)
    {
        var value = (user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub"))?.Value;
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    public static string? Username(this ClaimsPrincipal user) => user.FindFirst("username")?.Value;
}
