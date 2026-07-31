using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Core.UseCases.Internal;

public sealed class MembershipWriteService(
    IMembershipRepository memberships,
    MembershipNotifier notifier,
    ManagerElectionService election) : IInternalIntentOutcomeService, IInternalStarAwardService
{
    public void Ban(Guid membershipId, Guid intentId)
    {
        var membership = Require(membershipId);

        membership.Ban(intentId, DateTime.UtcNow);
        memberships.Update(membership);

        notifier.RolesChanged(membership);
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
            notifier.RolesChanged(currentManager);
        }
        notifier.RolesChanged(newManager);
    }

    public void Mute(Guid membershipId)
    {
        var membership = Require(membershipId);

        membership.Mute(DateTime.UtcNow);
        memberships.Update(membership);

        notifier.MuteChanged(membership);
    }

    public void AddStars(Guid membershipId, int stars)
    {
        var membership = Require(membershipId);

        membership.AddStars(stars);
        memberships.Update(membership);
    }

    private Membership Require(Guid membershipId) =>
        memberships.GetById(membershipId) ?? throw new NotFoundException("Membership not found.");
}
