namespace Zajednica.Identity.Core.UseCases;

/// <summary>
/// Produces the short-lived access JWT for an account. No roles are embedded — roles are
/// per-community and live on Community's Membership, so they are resolved per request, not baked
/// into the token. Implemented in Infrastructure (JWT adapter).
/// </summary>
public interface IAccessTokenGenerator
{
    string Generate(Guid accountId, string username);
}
