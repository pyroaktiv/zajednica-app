using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Core.UseCases.Internal;

public sealed class MembershipWriteService(
    IMembershipRepository memberships,
    IRealtimePusher realtime,
    ManagerElectionService election) : IInternalIntentOutcomeService, IInternalStarAwardService
{
    public void Ban(Guid membershipId, Guid intentId)
    {
        var membership = Require(membershipId);

        membership.Ban(intentId, DateTime.UtcNow);
        memberships.Update(membership);

        PushRolesChanged(membership);
    }

    public void ElectManager(Guid membershipId)
    {
        var newManager = Require(membershipId);
        var currentManager = memberships.GetManager(newManager.CommunityId);

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
