using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Posts;

public class Comment : Entity
{
    public Guid PostId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public Guid? ParentCommentId { get; private set; }
    public string Text { get; private set; } = null!;
    public bool HasReplies { get; private set; }
    public DateTime Date { get; private set; }

    private Comment() { }

    internal Comment(Guid postId, Guid authorMembershipId, Guid? parentCommentId, string text, DateTime date)
    {
        if (authorMembershipId == Guid.Empty)
            throw new EntityValidationException("AuthorMembershipId is required.");

        PostId = postId;
        AuthorMembershipId = authorMembershipId;
        ParentCommentId = parentCommentId;
        Text = text;
        Date = date;
    }

    internal void MarkHasReplies() => HasReplies = true;
}
