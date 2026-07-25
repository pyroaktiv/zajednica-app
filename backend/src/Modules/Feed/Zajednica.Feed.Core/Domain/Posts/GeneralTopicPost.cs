namespace Zajednica.Feed.Core.Domain.Posts;

public class GeneralTopicPost : Post
{
    public GeneralPostKind Kind { get; private set; }

    private GeneralTopicPost() { }

    public GeneralTopicPost(Guid communityId, Guid authorMembershipId, string text, GeneralPostKind kind,
        IEnumerable<string>? imageUrls, DateTime now)
        : base(communityId, authorMembershipId, text, imageUrls, now)
    {
        Kind = kind;
    }

    public override bool AllowsComments() => true;
}
