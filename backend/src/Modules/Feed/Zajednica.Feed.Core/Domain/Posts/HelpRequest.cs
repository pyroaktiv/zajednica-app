using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Posts;

public class HelpRequest : Post
{
    public bool Closed { get; private set; }

    private HelpRequest() { }

    public HelpRequest(Guid communityId, Guid authorMembershipId, string text, IEnumerable<string>? imageUrls, DateTime now)
        : base(communityId, authorMembershipId, text, imageUrls, now) { }

    public override bool AllowsComments() => false;

    public void Close(Guid actorMembershipId)
    {
        if (actorMembershipId != AuthorMembershipId)
            throw new ForbiddenException("Only the author can close a help request for further responses.");
        if (Closed)
            throw new EntityValidationException("Help request is already closed.");

        Closed = true;
    }
}
