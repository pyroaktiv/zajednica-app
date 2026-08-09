using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Core.UseCases.Internal;

public sealed class InternalMembershipCommandService(
    IMembershipRepository membershipRepository,
    MembershipNotifier membershipNotifier,
    ManagerElectionService electionService) : IInternalMembershipCommandService
{
    public void Ban(Guid membershipId, Guid intentId)
    {
        var membership = Require(membershipId);

        membership.Ban(intentId, DateTime.UtcNow);
        membershipRepository.Update(membership);

        membershipNotifier.RolesChanged(membership);
    }

    public void ElectManager(Guid membershipId)
    {
        var newManager = Require(membershipId);
        var currentManager = membershipRepository.GetManager(newManager.CommunityId);

        electionService.Elect(currentManager, newManager, DateTime.UtcNow);

        membershipRepository.Update(newManager);
        if (currentManager is not null)
        {
            membershipRepository.Update(currentManager);
            membershipNotifier.RolesChanged(currentManager);
        }
        membershipNotifier.RolesChanged(newManager);
    }

    public void Mute(Guid membershipId)
    {
        var membership = Require(membershipId);

        membership.Mute(DateTime.UtcNow);
        membershipRepository.Update(membership);

        membershipNotifier.MuteChanged(membership);
    }

    public void AddStars(Guid membershipId, int stars)
    {
        var membership = Require(membershipId);

        membership.AddStars(stars);
        membershipRepository.Update(membership);
    }

    private Membership Require(Guid membershipId) =>
        membershipRepository.GetById(membershipId) ?? throw new NotFoundException("Membership not found.");
}
