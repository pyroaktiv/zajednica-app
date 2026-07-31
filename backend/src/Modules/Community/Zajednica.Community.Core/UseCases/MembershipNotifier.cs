using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Core.UseCases;

public sealed class MembershipNotifier(IRealtimePusher realtime)
{
    public void RolesChanged(Membership membership) =>
        Push(membership, "membership.roles.changed");

    public void MuteChanged(Membership membership) =>
        Push(membership, "membership.mute.changed");

    private void Push(Membership membership, string @event) =>
        realtime.PushToUser(membership.AccountId,
            new RealtimeMessage(@event, new { communityId = membership.CommunityId }));
}
