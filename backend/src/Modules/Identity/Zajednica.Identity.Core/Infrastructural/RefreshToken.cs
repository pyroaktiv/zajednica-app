using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Identity.Core.Infrastructural;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }

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
