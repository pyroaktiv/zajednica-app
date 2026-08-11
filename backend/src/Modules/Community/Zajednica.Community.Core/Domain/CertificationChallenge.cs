using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class CertificationChallenge : AggregateRoot
{
    public Guid CommunityId { get; private set; }
    public Guid IssuerMembershipId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }

    private CertificationChallenge() { }

    public CertificationChallenge(Guid communityId, Guid issuerMembershipId, string token, DateTime expiresAt)
    {
        if (communityId == Guid.Empty)
            throw new EntityValidationException("CommunityId is required.");
        if (issuerMembershipId == Guid.Empty)
            throw new EntityValidationException("IssuerMembershipId is required.");
        if (string.IsNullOrWhiteSpace(token))
            throw new EntityValidationException("Token is required.");

        CommunityId = communityId;
        IssuerMembershipId = issuerMembershipId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public bool IsValid(DateTime now) => now < ExpiresAt;
}
