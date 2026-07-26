using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Api.Dto.Comments;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.Mappers;

namespace Zajednica.Feed.Core.UseCases;

public sealed class CommentService(
    IPostRepository posts,
    IInternalMembershipService memberships,
    INotificationSender notifications,
    IRealtimePusher realtime,
    AuthorDirectory authors,
    CommunityAccess access) : ICommentService
{
    public CommentDto Add(Guid accountId, Guid communityId, Guid postId, AddCommentRequest request)
    {
        var author = access.RequireConfirmed(accountId, communityId);
        var post = Require(postId, communityId);

        var comment = post.AddComment(author.MembershipId, request.Text, DateTime.UtcNow);
        posts.Update(post);

        Notify(post.AuthorMembershipId, author.MembershipId, "Novi komentar",
            "Neko je komentarisao vašu objavu.");
        PushChanged(post);

        return Single(comment);
    }

    public CommentDto Reply(Guid accountId, Guid communityId, Guid postId, Guid commentId,
        AddCommentRequest request)
    {
        var author = access.RequireConfirmed(accountId, communityId);
        var post = Require(postId, communityId);

        var parent = posts.GetComment(postId, commentId)
            ?? throw new NotFoundException("Comment not found on this post.");

        var reply = post.AddReply(parent, author.MembershipId, request.Text, DateTime.UtcNow);
        posts.Update(post);

        Notify(parent.AuthorMembershipId, author.MembershipId, "Novi odgovor",
            "Neko je odgovorio na vaš komentar.");
        PushChanged(post);

        return Single(reply);
    }

    public CursorPage<CommentDto> GetRoots(Guid accountId, Guid communityId, Guid postId, DateTime? after,
        int limit)
    {
        access.RequireConfirmed(accountId, communityId);
        Require(postId, communityId);

        return Page(postId, null, after, limit);
    }

    public CursorPage<CommentDto> GetReplies(Guid accountId, Guid communityId, Guid postId, Guid commentId,
        DateTime? after, int limit)
    {
        access.RequireConfirmed(accountId, communityId);
        Require(postId, communityId);

        return Page(postId, commentId, after, limit);
    }

    private CursorPage<CommentDto> Page(Guid postId, Guid? parentCommentId, DateTime? after, int limit)
    {
        var page = posts.GetCommentPage(postId, parentCommentId, after, Paging.Clamp(limit));
        var profiles = authors.For(page.Items.Select(c => c.AuthorMembershipId).ToList());

        return page.ToDtoPage(profiles);
    }

    private Post Require(Guid postId, Guid communityId)
    {
        var post = posts.Get(postId);
        if (post is null || post.CommunityId != communityId)
            throw new NotFoundException("Post not found in this community.");

        return post;
    }

    private CommentDto Single(Comment comment)
    {
        var profiles = authors.For([comment.AuthorMembershipId]);
        return comment.ToDto(profiles.GetValueOrDefault(comment.AuthorMembershipId));
    }

    private void Notify(Guid recipientMembershipId, Guid actorMembershipId, string title, string body)
    {
        if (recipientMembershipId == actorMembershipId)
            return;

        var recipient = (memberships.GetContexts([recipientMembershipId])).SingleOrDefault();
        if (recipient is null)
            return;

        notifications.Send(
            new NotificationRequest(recipient.AccountId, title, body, NotificationPriority.Default));
    }

    private void PushChanged(Post post) =>
        realtime.PushToChannel(Channels.Community(post.CommunityId),
            new RealtimeMessage("post.comments.changed", new { postId = post.Id }));
}
