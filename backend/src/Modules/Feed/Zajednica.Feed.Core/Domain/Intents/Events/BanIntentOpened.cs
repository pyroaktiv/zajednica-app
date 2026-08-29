using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed class BanIntentOpened : UserTargetingIntentOpened
{
    private BanIntentOpened() { }

    public BanIntentOpened(BanInitiative initiative, DateTime now) : base(initiative, now) { }

    public override Initiative ToInitiative() =>
        new BanInitiative(TargetStanding(), CommunityId, AuthorMembershipId, EligibleVoterCount, Description);
}
