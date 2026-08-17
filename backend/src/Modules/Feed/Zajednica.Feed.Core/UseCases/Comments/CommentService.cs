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
    IPostRepository postRepository,
    IFeedUnitOfWork unitOfWork,
    INotificationSender notificationSender,
    MemberDirectory memberDirectory,
    MemberRequirementsService access) : ICommentService
{
    public CommentDto Add(Guid accountId, Guid communityId, Guid postId, AddCommentRequestDto requestDto)
    {
        var authorMembershipId = access.RequireUnmutedConfirmed(accountId, communityId);
        var post = Require(postId, communityId);

        var comment = post.AddComment(authorMembershipId, requestDto.Text, DateTime.UtcNow);
        unitOfWork.Save();

        Notify(post.AuthorMembershipId, authorMembershipId, "Novi komentar",
            "Neko je komentarisao vašu objavu.");

        return Single(comment);
    }

    public CommentDto Reply(Guid accountId, Guid communityId, Guid postId, Guid commentId,
        AddCommentRequestDto requestDto)
    {
        var authorMembershipId = access.RequireUnmutedConfirmed(accountId, communityId);
        var post = RequireWithComment(postId, communityId, commentId);

        var reply = post.AddReply(commentId, authorMembershipId, requestDto.Text, DateTime.UtcNow);
        unitOfWork.Save();

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
        var page = postRepository.GetCommentPage(postId, parentCommentId, after, Paging.Clamp(limit));
        var profiles = memberDirectory.Profiles(page.Items.Select(c => c.AuthorMembershipId).ToList());

        return page.ToDtoPage(profiles);
    }

    private Post Require(Guid postId, Guid communityId) =>
        Require(postRepository.Get(postId), communityId);

    private Post RequireWithComment(Guid postId, Guid communityId, Guid commentId) =>
        Require(postRepository.GetWithComment(postId, commentId), communityId);

    private static Post Require(Post? post, Guid communityId)
    {
        if (post is null || post.CommunityId != communityId)
            throw new NotFoundException("Post not found in this community.");

        return post;
    }

    private void RequireExists(Guid postId, Guid communityId)
    {
        if (!postRepository.Exists(postId, communityId))
            throw new NotFoundException("Post not found in this community.");
    }

    private CommentDto Single(Comment comment)
    {
        var profiles = memberDirectory.Profiles([comment.AuthorMembershipId]);
        return comment.ToDto(profiles.GetValueOrDefault(comment.AuthorMembershipId));
    }

    private void Notify(Guid recipientMembershipId, Guid actorMembershipId, string title, string body)
    {
        if (recipientMembershipId == actorMembershipId)
            return;

        if (memberDirectory.AccountId(recipientMembershipId) is not { } recipientAccountId)
            return;

        notificationSender.Send(
            new NotificationRequest(recipientAccountId, title, body, NotificationChannel.Posts));
    }
}
