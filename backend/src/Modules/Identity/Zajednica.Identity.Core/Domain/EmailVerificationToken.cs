using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Identity.Core.Domain;

public class EmailVerificationToken : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }

    // EF
    private EmailVerificationToken() { }

    public EmailVerificationToken(Guid accountId, string token, DateTime expiresAt)
    {
        if (accountId == Guid.Empty)
            throw new EntityValidationException("AccountId is required.");
        if (string.IsNullOrWhiteSpace(token))
            throw new EntityValidationException("Token is required.");

        AccountId = accountId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    /// <summary>A screenshot/stale link guard: the token is usable only before it expires.</summary>
    public bool IsValid(DateTime now) => now < ExpiresAt;
}
