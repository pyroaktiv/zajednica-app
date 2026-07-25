using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Posts;

public abstract class Post : AggregateRoot
{
    private readonly List<Image> _images = [];
    private readonly List<Comment> _comments = [];

    public Guid CommunityId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public string Text { get; private set; } = null!;
    public DateTime DateCreated { get; private set; }
    public IReadOnlyList<Image> Images => _images;
    public IReadOnlyList<Comment> Comments => _comments;

    protected Post() { }

    protected Post(Guid communityId, Guid authorMembershipId, string text, IEnumerable<string>? imageUrls, DateTime now)
    {
        if (communityId == Guid.Empty)
            throw new EntityValidationException("CommunityId is required.");
        if (authorMembershipId == Guid.Empty)
            throw new EntityValidationException("AuthorMembershipId is required.");

        CommunityId = communityId;
        AuthorMembershipId = authorMembershipId;
        Text = RequireText(text);
        DateCreated = now;

        foreach (var url in imageUrls ?? [])
            _images.Add(new Image(url));
    }

    public abstract bool AllowsComments();

    public Comment AddComment(Guid authorMembershipId, string text, DateTime now)
    {
        RequireComments();

        var comment = new Comment(Id, authorMembershipId, null, RequireText(text), now);
        _comments.Add(comment);
        return comment;
    }

    public Comment AddReply(Comment parent, Guid authorMembershipId, string text, DateTime now)
    {
        RequireComments();

        if (parent.PostId != Id)
            throw new EntityValidationException("The parent comment belongs to another post.");
        if (parent.ParentCommentId is not null)
            throw new EntityValidationException("A reply cannot be answered, only the comment it replies to.");

        var reply = new Comment(Id, authorMembershipId, parent.Id, RequireText(text), now);
        _comments.Add(reply);
        return reply;
    }

    private void RequireComments()
    {
        if (!AllowsComments())
            throw new EntityValidationException("This kind of post does not allow comments.");
    }

    private static string RequireText(string text)
    {
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new EntityValidationException("Text is required.");
        return trimmed;
    }
}
