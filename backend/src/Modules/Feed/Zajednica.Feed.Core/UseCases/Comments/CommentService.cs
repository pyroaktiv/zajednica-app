using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Comments;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.Mappers;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Core.UseCases.Comments;

public sealed class CommentService(
    IPostRepository posts,
    INotificationSender notifications,
    IRealtimePusher realtime,
    MemberDirectory directory,
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
        PushChanged(communityId, postId);

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
        PushChanged(communityId, postId);

        return Single(reply);
    }

    public CursorPage<CommentDto> GetRoots(Guid accountId, Guid communityId, Guid postId, DateTime? after,
        int limit)
    {
        access.RequireConfirmed(accountId, communityId);
        RequireExists(postId, communityId);

        return Page(postId, null, after, limit);
    }

    public CursorPage<CommentDto> GetReplies(Guid accountId, Guid communityId, Guid postId, Guid commentId,
        DateTime? after, int limit)
    {
        access.RequireConfirmed(accountId, communityId);
        RequireExists(postId, communityId);

        return Page(postId, commentId, after, limit);
    }

    private CursorPage<CommentDto> Page(Guid postId, Guid? parentCommentId, DateTime? after, int limit)
    {
        var page = posts.GetCommentPage(postId, parentCommentId, after, Paging.Clamp(limit));
        var profiles = directory.Profiles(page.Items.Select(c => c.AuthorMembershipId).ToList());

        return page.ToDtoPage(profiles);
    }

    private Post Require(Guid postId, Guid communityId)
    {
        var post = posts.Get(postId);
        if (post is null || post.CommunityId != communityId)
            throw new NotFoundException("Post not found in this community.");

        return post;
    }

    private void RequireExists(Guid postId, Guid communityId)
    {
        if (!posts.Exists(postId, communityId))
            throw new NotFoundException("Post not found in this community.");
    }

    private CommentDto Single(Comment comment)
    {
        var profiles = directory.Profiles([comment.AuthorMembershipId]);
        return comment.ToDto(profiles.GetValueOrDefault(comment.AuthorMembershipId));
    }

    private void Notify(Guid recipientMembershipId, Guid actorMembershipId, string title, string body)
    {
        if (recipientMembershipId == actorMembershipId)
            return;

        var recipient = directory.Context(recipientMembershipId);
        if (recipient is null)
            return;

        notifications.Send(
            new NotificationRequest(recipient.AccountId, title, body, NotificationPriority.Default));
    }

    private void PushChanged(Guid communityId, Guid postId) =>
        realtime.PushToChannel(Channels.Community(communityId),
            new RealtimeMessage("post.comments.changed", new { postId }));
}
