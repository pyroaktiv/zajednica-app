using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Identity.Core.Domain;

/// <summary>
/// Persisted refresh token backing JWT rotation. Login issues one; every refresh rotates it (the
/// presented token row is removed and a fresh one inserted); logout removes it. There is no revoked
/// flag — a rotated/expired token is simply absent, so presenting it fails the lookup. Separate
/// aggregate root with its own repository and lifecycle.
/// Production hardening (hashing the stored value, reuse-detection across a token family) is one
/// sentence in the thesis, not code.
/// </summary>
public class RefreshToken : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }

    // EF
    private RefreshToken() { }

    public RefreshToken(Guid accountId, string token, DateTime expiresAt)
    {
        if (accountId == Guid.Empty)
            throw new EntityValidationException("AccountId is required.");
        if (string.IsNullOrWhiteSpace(token))
            throw new EntityValidationException("Token is required.");

        AccountId = accountId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public bool IsValid(DateTime now) => now < ExpiresAt;
}
