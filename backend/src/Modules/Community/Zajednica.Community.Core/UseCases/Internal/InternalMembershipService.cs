using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;

namespace Zajednica.Community.Core.UseCases.Internal;

public sealed class InternalMembershipService(
    IMembershipRepository memberships,
    IBlacklistRepository blacklist,
    IRealtimePusher realtime,
    ManagerElectionService election,
    MembershipBanService ban) : IInternalMembershipService
{
    public MembershipContextDto? GetContext(Guid accountId, Guid communityId) =>
        (memberships.Get(accountId, communityId))?.ToContextDto();

    public IReadOnlyList<MembershipContextDto> GetContexts(IReadOnlyCollection<Guid> membershipIds)
    {
        if (membershipIds.Count == 0)
            return [];

        var found = memberships.GetManyByIds(membershipIds);
        return found.Select(m => m.ToContextDto()).ToList();
    }

    public IReadOnlyList<MembershipContextDto> GetConfirmed(Guid communityId) =>
        (memberships.GetByCommunity(communityId))
        .Where(m => m.IsActive() && m.IsConfirmed())
        .Select(m => m.ToContextDto())
        .ToList();

    public int GetConfirmedCount(Guid communityId) =>
        memberships.CountConfirmed(communityId);

    public bool AreEligible(IReadOnlyCollection<Guid> membershipIds)
    {
        if (membershipIds.Count == 0)
            return false;

        var found = memberships.GetManyByIds(membershipIds);
        return found.Count == membershipIds.Distinct().Count()
               && found.All(m => m.IsActive() && m.IsConfirmed());
    }

    public void Ban(Guid membershipId, Guid intentId)
    {
        var membership = Require(membershipId);

        var entry = ban.Ban(membership, intentId, DateTime.UtcNow);
        memberships.Update(membership);
        blacklist.Add(entry);

        PushRolesChanged(membership);
    }

    public void ElectManager(Guid membershipId)
    {
        var newManager = Require(membershipId);

        var currentManager = (memberships.GetByCommunity(newManager.CommunityId))
            .SingleOrDefault(m => m.HasRole(CommunityRole.Manager));

        election.Elect(currentManager, newManager, DateTime.UtcNow);

        memberships.Update(newManager);
        if (currentManager is not null)
        {
            memberships.Update(currentManager);
            PushRolesChanged(currentManager);
        }
        PushRolesChanged(newManager);
    }

    public void AddStars(Guid membershipId, int stars)
    {
        var membership = Require(membershipId);

        membership.AddStars(stars);
        memberships.Update(membership);
    }

    private Membership Require(Guid membershipId) =>
        memberships.GetById(membershipId) ?? throw new NotFoundException("Membership not found.");

    private void PushRolesChanged(Membership membership) =>
        realtime.PushToUser(membership.AccountId,
            new RealtimeMessage("membership.roles.changed", new { communityId = membership.CommunityId }));
}
