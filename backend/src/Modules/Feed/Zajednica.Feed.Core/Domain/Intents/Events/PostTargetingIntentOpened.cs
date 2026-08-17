using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed class PostTargetingIntentOpened : IntentOpened
{
    public Guid PostId { get; private set; }

    private PostTargetingIntentOpened() { }

    public PostTargetingIntentOpened(PostTargetingInitiative initiative, DateTime now) : base(initiative, now)
    {
        PostId = initiative.PostId;
    }

    public override Initiative ToInitiative() =>
        new PostTargetingInitiative(PostId, CommunityId, AuthorMembershipId, EligibleVoterCount, Description);
}
