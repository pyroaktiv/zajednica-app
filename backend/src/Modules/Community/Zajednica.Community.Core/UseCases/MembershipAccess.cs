using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using CommunityAggregate = Zajednica.Community.Core.Domain.Community;

namespace Zajednica.Community.Core.UseCases;

public sealed class MembershipAccess(ICommunityRepository communities, IMembershipRepository memberships)
{
    public async Task<(CommunityAggregate Community, Membership Membership)> RequireMemberAsync(
        Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        var community = await communities.GetByIdAsync(communityId, ct)
            ?? throw new NotFoundException("Community not found.");

        var membership = await memberships.GetAsync(accountId, communityId, ct)
            ?? throw new ForbiddenException("Not a member of this community.");

        if (!membership.IsActive())
            throw new ForbiddenException("Membership is not active.");

        return (community, membership);
    }

    public async Task<(CommunityAggregate Community, Membership Membership)> RequireConfirmedAsync(
        Guid accountId, Guid communityId, CancellationToken ct = default)
    {
        var access = await RequireMemberAsync(accountId, communityId, ct);

        if (!access.Membership.IsConfirmed())
            throw new ForbiddenException("Only a confirmed member can do this.");

        return access;
    }

    public Task<(CommunityAggregate Community, Membership Membership)> RequireRoleAsync(
        Guid accountId, Guid communityId, CommunityRole role, CancellationToken ct = default) =>
        RequireAnyRoleAsync(accountId, communityId, ct, role);

    public async Task<(CommunityAggregate Community, Membership Membership)> RequireAnyRoleAsync(
        Guid accountId, Guid communityId, CancellationToken ct = default, params CommunityRole[] roles)
    {
        var access = await RequireConfirmedAsync(accountId, communityId, ct);

        if (!roles.Any(access.Membership.HasRole))
            throw new ForbiddenException($"Requires one of the roles: {string.Join(", ", roles)}.");

        return access;
    }
}
