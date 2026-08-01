using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Core.Domain;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Core.UseCases;

public sealed class CommunityAccess(IInternalMembershipAccessService memberships)
{
    public Guid RequireConfirmed(Guid accountId, Guid communityId) =>
        memberships.RequireConfirmedMembershipId(accountId, communityId);

    public VoterContext RequireVoter(Guid accountId, Guid communityId)
    {
        var voter = memberships.RequireConfirmedVoter(accountId, communityId);
        return new VoterContext(voter.MembershipId, voter.CertifiedAt);
    }

    public Guid RequireUnmutedConfirmed(Guid accountId, Guid communityId) =>
        memberships.RequireUnmutedConfirmedMembershipId(accountId, communityId);

    public MembershipStatus StatusOf(Guid communityId, Guid membershipId) =>
        memberships.IsConfirmedMemberOf(communityId, membershipId)
            ? MembershipStatus.Confirmed
            : MembershipStatus.Unconfirmed;
}
