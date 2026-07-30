using Zajednica.Community.Api.Internal;

namespace Zajednica.Feed.Core.UseCases;

public sealed class CommunityAccess(IInternalMembershipAccessService memberships)
{
    public Guid RequireConfirmed(Guid accountId, Guid communityId) =>
        memberships.RequireConfirmedMembershipId(accountId, communityId);

    public bool IsConfirmedMember(Guid communityId, Guid membershipId) =>
        memberships.IsConfirmedMemberOf(communityId, membershipId);
}
