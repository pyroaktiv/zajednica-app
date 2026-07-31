using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Notifications;
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
    MemberDirectory directory,
    CommunityAccess access) : ICommentService
{
    public CommentDto Add(Guid accountId, Guid communityId, Guid postId, AddCommentRequest request)
    {
        var authorMembershipId = access.RequireUnmutedConfirmed(accountId, communityId);
        var post = Require(postId, communityId);

        var comment = post.AddComment(authorMembershipId, request.Text, DateTime.UtcNow);
        posts.Update(post);

        Notify(post.AuthorMembershipId, authorMembershipId, "Novi komentar",
            "Neko je komentarisao vašu objavu.");

        return Single(comment);
    }

    public CommentDto Reply(Guid accountId, Guid communityId, Guid postId, Guid commentId,
        AddCommentRequest request)
    {
        var authorMembershipId = access.RequireUnmutedConfirmed(accountId, communityId);
        var post = RequireWithComment(postId, communityId, commentId);

        var reply = post.AddReply(commentId, authorMembershipId, request.Text, DateTime.UtcNow);
        posts.Update(post);

        Notify(post.Comments.Single(c => c.Id == commentId).AuthorMembershipId, authorMembershipId,
            "Novi odgovor", "Neko je odgovorio na vaš komentar.");

        return Single(reply);
    }

    public CursorPage<CommentDto, PageCursor> GetRoots(Guid accountId, Guid communityId, Guid postId, PageCursor? after,
        int limit)
    {
        access.RequireConfirmed(accountId, communityId);
        RequireExists(postId, communityId);

        return Page(postId, null, after, limit);
    }

    public CursorPage<CommentDto, PageCursor> GetReplies(Guid accountId, Guid communityId, Guid postId, Guid commentId,
        PageCursor? after, int limit)
    {
        access.RequireConfirmed(accountId, communityId);
        RequireExists(postId, communityId);

        return Page(postId, commentId, after, limit);
    }

    private CursorPage<CommentDto, PageCursor> Page(Guid postId, Guid? parentCommentId, PageCursor? after, int limit)
    {
        var page = posts.GetCommentPage(postId, parentCommentId, after, Paging.Clamp(limit));
        var profiles = directory.Profiles(page.Items.Select(c => c.AuthorMembershipId).ToList());

        return page.ToDtoPage(profiles);
    }

    private Post Require(Guid postId, Guid communityId) =>
        Require(posts.Get(postId), communityId);

    private Post RequireWithComment(Guid postId, Guid communityId, Guid commentId) =>
        Require(posts.GetWithComment(postId, commentId), communityId);

    private static Post Require(Post? post, Guid communityId)
    {
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

        if (directory.AccountId(recipientMembershipId) is not { } recipientAccountId)
            return;

        notifications.Send(
            new NotificationRequest(recipientAccountId, title, body, NotificationPriority.Default));
    }
}
