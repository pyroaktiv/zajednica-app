using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed class MuteIntentOpened : UserTargetingIntentOpened
{
    private MuteIntentOpened() { }

    public MuteIntentOpened(MuteInitiative initiative, DateTime now) : base(initiative, now) { }

    public override Initiative ToInitiative() =>
        new MuteInitiative(TargetStanding(), CommunityId, AuthorMembershipId, EligibleVoterCount, Description);
}
