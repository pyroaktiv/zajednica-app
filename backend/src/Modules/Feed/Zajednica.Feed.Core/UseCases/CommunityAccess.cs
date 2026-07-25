using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;

namespace Zajednica.Feed.Core.UseCases;

public sealed class CommunityAccess(IInternalMembershipService memberships)
{
    public MembershipContextDto RequireConfirmed(Guid accountId, Guid communityId)
    {
        var context = memberships.GetContext(accountId, communityId)
            ?? throw new ForbiddenException("Not a member of this community.");

        if (!context.IsActive)
            throw new ForbiddenException("Membership is not active.");
        if (!context.IsConfirmed)
            throw new ForbiddenException("Only a confirmed member can take part in the feed.");

        return context;
    }
}
