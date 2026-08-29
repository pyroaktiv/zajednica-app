using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed class PostRatingIntentOpened : IntentOpened
{
    public Guid PostId { get; private set; }

    private PostRatingIntentOpened() { }

    public PostRatingIntentOpened(PostRatingInitiative initiative, DateTime now) : base(initiative, now)
    {
        PostId = initiative.PostId;
    }

    public override Initiative ToInitiative() =>
        new PostRatingInitiative(PostId, CommunityId, AuthorMembershipId, EligibleVoterCount, Description);
}
