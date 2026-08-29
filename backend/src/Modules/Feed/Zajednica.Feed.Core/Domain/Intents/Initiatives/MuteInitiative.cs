using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents.Initiatives;

public sealed class MuteInitiative : UserTargetingInitiative
{
    public MuteInitiative(MemberStandingContext target, Guid communityId, Guid authorMembershipId,
        int eligibleVoterCount, string description)
        : base(target, communityId, authorMembershipId, eligibleVoterCount, description)
    {
    }

    public override string KindName => "Mute";

    public override IntentOpened ToOpenedEvent(DateTime now) => new MuteIntentOpened(this, now);
}
