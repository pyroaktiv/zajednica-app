using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class Certificate : AggregateRoot
{
    public Guid CommunityId { get; private set; }
    public Guid IssuerMembershipId { get; private set; }
    public Guid CandidateMembershipId { get; private set; }
    public DateTime Date { get; private set; }

    // EF
    private Certificate() { }

    public Certificate(Guid communityId, Guid issuerMembershipId, Guid candidateMembershipId, DateTime date)
    {
        if (communityId == Guid.Empty)
            throw new EntityValidationException("CommunityId is required.");
        if (issuerMembershipId == Guid.Empty)
            throw new EntityValidationException("IssuerMembershipId is required.");
        if (candidateMembershipId == Guid.Empty)
            throw new EntityValidationException("CandidateMembershipId is required.");
        if (issuerMembershipId == candidateMembershipId)
            throw new EntityValidationException("A member cannot certify themselves.");

        CommunityId = communityId;
        IssuerMembershipId = issuerMembershipId;
        CandidateMembershipId = candidateMembershipId;
        Date = date;
    }
}
