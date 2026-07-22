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
    private const int MaxPageSize = 50;

    public async Task<CommentDto> AddAsync(Guid accountId, Guid communityId, Guid postId, AddCommentRequest request,
        CancellationToken ct = default)
    {
        var author = await access.RequireConfirmedAsync(accountId, communityId, ct);
        var post = await RequireAsync(postId, communityId, ct);

        var comment = post.AddComment(author.MembershipId, request.Text, DateTime.UtcNow);
        await posts.UpdateAsync(post, ct);

        await NotifyAsync(post.AuthorMembershipId, author.MembershipId, "Novi komentar",
            "Neko je komentarisao vašu objavu.", ct);
        await PushChangedAsync(post, ct);

        return await SingleAsync(comment, ct);
    }

    public async Task<CommentDto> ReplyAsync(Guid accountId, Guid communityId, Guid postId, Guid commentId,
        AddCommentRequest request, CancellationToken ct = default)
    {
        var author = await access.RequireConfirmedAsync(accountId, communityId, ct);
        var post = await RequireAsync(postId, communityId, ct);

        var parent = await posts.GetCommentAsync(postId, commentId, ct)
            ?? throw new NotFoundException("Comment not found on this post.");

        var reply = post.AddReply(parent, author.MembershipId, request.Text, DateTime.UtcNow);
        await posts.UpdateAsync(post, ct);

        await NotifyAsync(parent.AuthorMembershipId, author.MembershipId, "Novi odgovor",
            "Neko je odgovorio na vaš komentar.", ct);
        await PushChangedAsync(post, ct);

        return await SingleAsync(reply, ct);
    }

    public async Task<CursorPage<CommentDto>> GetRootsAsync(Guid accountId, Guid communityId, Guid postId, string? cursor,
        int limit, CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);
        await RequireAsync(postId, communityId, ct);

        return await PageAsync(postId, null, cursor, limit, ct);
    }

    public async Task<CursorPage<CommentDto>> GetRepliesAsync(Guid accountId, Guid communityId, Guid postId, Guid commentId,
        string? cursor, int limit, CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);
        await RequireAsync(postId, communityId, ct);

        return await PageAsync(postId, commentId, cursor, limit, ct);
    }

    private async Task<CursorPage<CommentDto>> PageAsync(Guid postId, Guid? parentCommentId, string? cursor, int limit,
        CancellationToken ct)
    {
        var page = await posts.GetCommentPageAsync(postId, parentCommentId, Cursor.Decode(cursor), Clamp(limit), ct);
        var profiles = await authors.ForAsync(page.Items.Select(c => c.AuthorMembershipId).ToList(), ct);

        return page.ToDtoPage(profiles);
    }

    private async Task<Post> RequireAsync(Guid postId, Guid communityId, CancellationToken ct)
    {
        var post = await posts.GetAsync(postId, ct);
        if (post is null || post.CommunityId != communityId)
            throw new NotFoundException("Post not found in this community.");

        return post;
    }

    private async Task<CommentDto> SingleAsync(Comment comment, CancellationToken ct)
    {
        var profiles = await authors.ForAsync([comment.AuthorMembershipId], ct);
        return comment.ToDto(profiles.GetValueOrDefault(comment.AuthorMembershipId));
    }

    private async Task NotifyAsync(Guid recipientMembershipId, Guid actorMembershipId, string title, string body,
        CancellationToken ct)
    {
        if (recipientMembershipId == actorMembershipId)
            return;

        var recipient = (await memberships.GetContextsAsync([recipientMembershipId], ct)).SingleOrDefault();
        if (recipient is null)
            return;

        await notifications.SendAsync(
            new NotificationRequest(recipient.AccountId, title, body, NotificationPriority.Default), ct);
    }

    private Task PushChangedAsync(Post post, CancellationToken ct) =>
        realtime.PushToChannelAsync(Channels.Community(post.CommunityId),
            new RealtimeMessage("post.comments.changed", new { postId = post.Id }), ct);

    private static int Clamp(int limit) => limit < 1 ? 20 : Math.Min(limit, MaxPageSize);
}
