using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Posts;

public class GeneralTopicPost : Post
{
    public GeneralPostKind Kind { get; private set; }
    public CommunityRating? Rating { get; private set; }

    private GeneralTopicPost() { }

    public GeneralTopicPost(Guid communityId, Guid authorMembershipId, string text, GeneralPostKind kind,
        IEnumerable<string>? imageUrls, DateTime now)
        : base(communityId, authorMembershipId, text, imageUrls, now)
    {
        Kind = kind;
    }

    public override bool AllowsComments() => true;

    public void RateByCommunity(CommunityRating rating)
    {
        if (Rating is not null)
            throw new EntityValidationException("This post already has a community rating.");

        Rating = rating;
    }
}
