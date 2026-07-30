using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Core.Domain;

namespace Zajednica.Feed.Core.UseCases;

public sealed class CommunityAccess(IInternalMembershipAccessService memberships)
{
    public Guid RequireConfirmed(Guid accountId, Guid communityId) =>
        memberships.RequireConfirmedMembershipId(accountId, communityId);

    public MembershipStatus StatusOf(Guid communityId, Guid membershipId) =>
        memberships.IsConfirmedMemberOf(communityId, membershipId)
            ? MembershipStatus.Confirmed
            : MembershipStatus.Unconfirmed;
}
