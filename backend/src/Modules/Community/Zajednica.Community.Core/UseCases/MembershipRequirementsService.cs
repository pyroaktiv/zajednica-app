using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using CommunityAggregate = Zajednica.Community.Core.Domain.Community;

namespace Zajednica.Community.Core.UseCases;

public sealed class MembershipRequirementsService(ICommunityRepository communityRepository, IMembershipRepository membershipRepository)
{
    public (CommunityAggregate Community, Membership Membership) RequireMember(
        Guid accountId, Guid communityId)
    {
        var community = communityRepository.GetById(communityId)
            ?? throw new NotFoundException("Community not found.");

        var membership = membershipRepository.Get(accountId, communityId)
            ?? throw new ForbiddenException("Not a member of this community.");

        if (!membership.IsActive())
            throw new ForbiddenException("Membership is not active.");

        return (community, membership);
    }

    public (CommunityAggregate Community, Membership Membership) RequireConfirmed(
        Guid accountId, Guid communityId)
    {
        var access = RequireMember(accountId, communityId);

        if (!access.Membership.IsConfirmed())
            throw new ForbiddenException("Only a confirmed member can do this.");

        return access;
    }

    public (CommunityAggregate Community, Membership Membership) RequireRole(
        Guid accountId, Guid communityId, CommunityRole role) =>
        RequireAnyRole(accountId, communityId, role);

    public (CommunityAggregate Community, Membership Membership) RequireAnyRole(
        Guid accountId, Guid communityId, params CommunityRole[] roles)
    {
        var access = RequireConfirmed(accountId, communityId);

        if (!roles.Any(access.Membership.HasRole))
            throw new ForbiddenException($"Requires one of the roles: {string.Join(", ", roles)}.");

        return access;
    }
}
