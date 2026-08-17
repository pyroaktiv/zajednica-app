using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents.Initiatives;

public sealed class PostTargetingInitiative : Initiative
{
    public Guid PostId { get; }

    public PostTargetingInitiative(Guid postId, Guid communityId, Guid authorMembershipId, int eligibleVoterCount,
        string description)
        : base(communityId, authorMembershipId, eligibleVoterCount, description)
    {
        if (postId == Guid.Empty)
            throw new EntityValidationException("A post-targeting initiative has to say which post it is about.");

        PostId = postId;
    }

    public override string KindName => "PostRating";

    public override IntentOpened ToOpenedEvent(DateTime now) => new PostTargetingIntentOpened(this, now);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var component in base.GetEqualityComponents())
            yield return component;

        yield return PostId;
    }
}
