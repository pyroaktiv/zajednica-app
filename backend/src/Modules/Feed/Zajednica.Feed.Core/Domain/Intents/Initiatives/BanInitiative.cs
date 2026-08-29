using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents.Initiatives;

public sealed class BanInitiative : UserTargetingInitiative
{
    public BanInitiative(MemberStandingContext target, Guid communityId, Guid authorMembershipId,
        int eligibleVoterCount, string description)
        : base(target, communityId, authorMembershipId, eligibleVoterCount, description)
    {
    }

    public override string KindName => "Ban";

    public override bool Supersedes(Initiative other) =>
        other is UserTargetingInitiative o
        && o.CommunityId == CommunityId
        && o.TargetMembershipId == TargetMembershipId;

    public override IntentOpened ToOpenedEvent(DateTime now) => new BanIntentOpened(this, now);

    protected override void EnsureSpecificTarget()
    {
        if (Target.Status == MembershipStatus.Banned)
            throw new EntityValidationException("This member is already banned.");
    }
}
