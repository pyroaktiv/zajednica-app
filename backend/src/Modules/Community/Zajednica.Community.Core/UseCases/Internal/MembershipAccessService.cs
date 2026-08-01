using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Core.UseCases.Internal;

public sealed class MembershipAccessService(IMembershipRepository memberships)
    : IInternalMembershipAccessService
{
    public Guid RequireActiveMembershipId(Guid accountId, Guid communityId) =>
        RequireActive(accountId, communityId).Id;

    public Guid RequireConfirmedMembershipId(Guid accountId, Guid communityId)
    {
        var membership = RequireActive(accountId, communityId);

        if (!membership.IsConfirmed())
            throw new ForbiddenException("Only a confirmed member can do this.");

        return membership.Id;
    }

    public ConfirmedVoterDto RequireConfirmedVoter(Guid accountId, Guid communityId)
    {
        var membership = RequireActive(accountId, communityId);

        if (!membership.IsConfirmed())
            throw new ForbiddenException("Only a confirmed member can do this.");

        return new ConfirmedVoterDto(membership.Id, membership.CertifiedAt ?? membership.DateJoined);
    }

    public Guid RequireUnconfirmedMembershipId(Guid accountId, Guid communityId)
    {
        var membership = RequireActive(accountId, communityId);

        if (membership.IsConfirmed())
            throw new ForbiddenException("Only an unconfirmed member can do this.");

        return membership.Id;
    }

    public Guid RequireUnmutedActiveMembershipId(Guid accountId, Guid communityId) =>
        RequireUnmuted(RequireActive(accountId, communityId)).Id;

    public Guid RequireUnmutedConfirmedMembershipId(Guid accountId, Guid communityId)
    {
        var membership = RequireActive(accountId, communityId);

        if (!membership.IsConfirmed())
            throw new ForbiddenException("Only a confirmed member can do this.");

        return RequireUnmuted(membership).Id;
    }

    public bool IsConfirmedMemberOf(Guid communityId, Guid membershipId) =>
        FindIn(communityId, membershipId) is { } membership
        && membership.IsActive()
        && membership.IsConfirmed();

    public bool CanIssueCertifications(Guid communityId, Guid membershipId) =>
        FindIn(communityId, membershipId) is { } membership
        && membership.IsActive()
        && membership.IsConfirmed()
        && membership.HasRole(CommunityRole.Issuer);

    private Membership RequireActive(Guid accountId, Guid communityId)
    {
        var membership = memberships.Get(accountId, communityId)
            ?? throw new ForbiddenException("Not a member of this community.");

        if (!membership.IsActive())
            throw new ForbiddenException("Membership is not active.");

        return membership;
    }

    private static Membership RequireUnmuted(Membership membership)
    {
        if (membership.IsMuted(DateTime.UtcNow))
            throw new ForbiddenException("You are muted in this community.");

        return membership;
    }

    private Membership? FindIn(Guid communityId, Guid membershipId)
    {
        var membership = memberships.GetById(membershipId);
        return membership?.CommunityId == communityId ? membership : null;
    }
}
