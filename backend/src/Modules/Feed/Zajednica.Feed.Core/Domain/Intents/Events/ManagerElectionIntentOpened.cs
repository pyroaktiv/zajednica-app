using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed class ManagerElectionIntentOpened : UserTargetingIntentOpened
{
    private ManagerElectionIntentOpened() { }

    public ManagerElectionIntentOpened(ManagerElectionInitiative initiative, DateTime now) : base(initiative, now) { }

    public override Initiative ToInitiative() =>
        new ManagerElectionInitiative(TargetStanding(), CommunityId, AuthorMembershipId, EligibleVoterCount, Description);
}
