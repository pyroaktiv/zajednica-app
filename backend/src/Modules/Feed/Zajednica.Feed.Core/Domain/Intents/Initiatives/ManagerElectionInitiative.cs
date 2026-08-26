using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents.Initiatives;

public sealed class ManagerElectionInitiative : UserTargetingInitiative
{
    public ManagerElectionInitiative(MemberStandingContext target, Guid communityId, Guid authorMembershipId,
        int eligibleVoterCount, string description)
        : base(target, communityId, authorMembershipId, eligibleVoterCount, description)
    {
    }

    public override string KindName => "ManagerElection";

    public override bool AreVotesPublic => true;

    public override IntentOpened ToOpenedEvent(DateTime now) => new ManagerElectionIntentOpened(this, now);

    protected override void EnsureSpecificTarget()
    {
        if (Target.Role == MembershipRole.Manager)
            throw new EntityValidationException("This member is already the manager.");
    }
}
